﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectComparer
{
    public class ObjectTracker<TResult>
        where TResult : class
    {
        private List<TResult> _differences;

        private Func<List<Difference>, List<TResult>> _customDeletion;
        private Func<List<Difference>, List<TResult>> _customAddition;
        private Func<List<Difference>, List<TResult>> _customAmendment;

        public ObjectTracker()
            :this(null, null, null)
        {
            
        }

        public ObjectTracker(
            Func<List<Difference>, List<TResult>> customAddition,
            Func<List<Difference>, List<TResult>> customDeletion,
            Func<List<Difference>, List<TResult>> customAmendment)
        {
            _differences = new List<TResult>();
            _customAddition = customAddition;
            _customDeletion = customDeletion;
            _customAmendment = customAmendment;
        }
        public ObjectTracker<TResult> SetCustomAddition(Func<List<Difference>, List<TResult>> customAddition)
        {
            _customAddition = customAddition;
            return this;
        }

        public ObjectTracker<TResult> SetCustomDeletion(Func<List<Difference>, List<TResult>> customDeletion)
        {
            _customDeletion = customDeletion;
            return this;
        }

        public ObjectTracker<TResult> SetCustomAmendment(Func<List<Difference>, List<TResult>> customAmendment)
        {
            _customAmendment= customAmendment;
            return this;
        }

        public ObjectTracker<TResult> TrackCollection<TObjectTypeToTrack, TKeyType, TPropertyType>(
            List<TObjectTypeToTrack> before, 
            List<TObjectTypeToTrack> after,
            Expression<Func<TObjectTypeToTrack, TKeyType>> keyExpression,
            params Expression<Func<TObjectTypeToTrack, TPropertyType>>[] fieldToTrackExpressions)
        {
            foreach (Expression<Func<TObjectTypeToTrack, TPropertyType>> fieldToTrackExpression in fieldToTrackExpressions)
            {
                List<TResult> additionDifferences = TrackAdditions(before, after, keyExpression, fieldToTrackExpression);
                List<TResult> deletionDifferences = TrackDeletions(before, after, keyExpression, fieldToTrackExpression);
                List<TResult> amendmentnDifferences = TrackAmendments(before, after, keyExpression, fieldToTrackExpression);

                _differences.AddRange(additionDifferences);
                _differences.AddRange(deletionDifferences);
                _differences.AddRange(amendmentnDifferences);
            }

            return this;
        }

        public ObjectTracker<TResult> Track<TObjectTypeToTrack, TPropertyType>(
            TObjectTypeToTrack before,
            TObjectTypeToTrack after,
            params Expression<Func<TObjectTypeToTrack, TPropertyType>>[] fieldToTrackExpressions)

        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            PropertyInfo[] beforePropertyInfos = before.GetType().GetProperties(bindingFlags);
            PropertyInfo[] afterPropertyInfos = after.GetType().GetProperties(bindingFlags);
            List<Difference> differences = new List<Difference>();

            for (int i = 0; i < beforePropertyInfos.Length; i++)
            {
                Type beforePropertyType = beforePropertyInfos[i].PropertyType;

                bool shouldProcessField = fieldToTrackExpressions.Any(expression => ((MemberExpression)expression.Body).Member.Name == beforePropertyType.Name);

                if(shouldProcessField && (beforePropertyType == typeof(string) || beforePropertyType.GetInterface("IEnumberable") == null))
                {
                    string oldValue = beforePropertyInfos[i].GetValue(before).ToString();
                    string newValue = afterPropertyInfos[i].GetValue(after).ToString();
                    TypeOfDifference typeOfDifference = TypeOfDifference.Amend;

                    if(oldValue == newValue)
                    {
                        continue;
                    }
                    else if(oldValue == null && newValue != null)
                    {
                        typeOfDifference = TypeOfDifference.Add;
                    }
                    else
                    {
                        typeOfDifference = TypeOfDifference.Delete;
                    }

                    Difference difference = new Difference()
                    {
                        Type = typeOfDifference,
                        PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{beforePropertyInfos[i].Name}",
                        NewValue = newValue,
                        OldValue = oldValue
                    };

                    differences.Add(difference);
                }
            }

            if (_customAddition == null)
                throw new ObjectTrackerException("Custom Addition procedure not set.");

            if (_customDeletion == null)
                throw new ObjectTrackerException("Custom Deletion procedure not set.");

            if (_customAmendment == null)
                throw new ObjectTrackerException("Custom Amendment procedure not set.");


            _differences.AddRange(_customAddition(differences.Where(x => x.Type == TypeOfDifference.Add).ToList()));
            _differences.AddRange(_customDeletion(differences.Where(x => x.Type == TypeOfDifference.Delete).ToList()));
            _differences.AddRange(_customAmendment(differences.Where(x => x.Type == TypeOfDifference.Amend).ToList()));


            return this;
        }

        private List<TResult> TrackAdditions<TObjectTypeToTrack, TKeyType, TPropertyType>(List<TObjectTypeToTrack> before, List<TObjectTypeToTrack> after, Expression<Func<TObjectTypeToTrack, TKeyType>> keyExpression, Expression<Func<TObjectTypeToTrack, TPropertyType>> fieldToTrackExpression)
        {
            List<TKeyType> addedKeys = after
                                            .Select(keyExpression.Compile())
                                            .Except(before.Select(keyExpression.Compile()))
                                            .ToList();

            List<TPropertyType> addedFields = after
                                                .Where(entity => addedKeys.Contains(keyExpression.Compile()(entity)))
                                                .Select(fieldToTrackExpression.Compile())
                                                .ToList();

            List<Difference> addedDifferences = new List<Difference>();

            for (int i = 0; i < addedKeys.Count; i++)
            {
                Difference addedDifference = new Difference()
                {
                    Type = TypeOfDifference.Add,
                    PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{((MemberExpression)fieldToTrackExpression.Body).Member.Name}",
                    NewValue = addedFields[i].ToString(),
                    OldValue = null
                };

                addedDifferences.Add(addedDifference);
            }

            if (_customAddition == null)
                throw new ObjectTrackerException("Custom Addition procedure not set.");

            return _customAddition(addedDifferences);
        }

        private List<TResult> TrackDeletions<TObjectTypeToTrack, TKeyType, TPropertyType>(
            List<TObjectTypeToTrack> before, 
            List<TObjectTypeToTrack> after, 
            Expression<Func<TObjectTypeToTrack, TKeyType>> keyExpression, 
            Expression<Func<TObjectTypeToTrack, TPropertyType>> fieldToTrackExpression)
        {
            List<TKeyType> deletedKeys = before
                                            .Select(keyExpression.Compile())
                                            .Except(after.Select(keyExpression.Compile()))
                                            .ToList();

            List<TPropertyType> deletedFields = before
                                                    .Where(entity => deletedKeys.Contains(keyExpression.Compile()(entity)))
                                                    .Select(fieldToTrackExpression.Compile())
                                                    .ToList();

            List<Difference> deletionDifferences = new List<Difference>();

            for (int i = 0; i < deletedKeys.Count; i++)
            {
                Difference deletionDifference = new Difference()
                {
                    Type = TypeOfDifference.Delete,
                    PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{((MemberExpression)fieldToTrackExpression.Body).Member.Name}",
                    NewValue = null,
                    OldValue = deletedFields[i].ToString()
                };

                deletionDifferences.Add(deletionDifference);
            }

            if (_customDeletion == null)
                throw new ObjectTrackerException("Custom Deletion procedure not set.");

            return _customDeletion(deletionDifferences);
        }

        private List<TResult> TrackAmendments<TObjectTypeToTrack, TKeyType, TPropertyType>(
            List<TObjectTypeToTrack> before,
            List<TObjectTypeToTrack> after,
            Expression<Func<TObjectTypeToTrack, TKeyType>> keyExpression,
            Expression<Func<TObjectTypeToTrack, TPropertyType>> fieldToTrackExpression)
        {
            List<TKeyType> amendedKeys = after
                                            .Select(keyExpression.Compile())
                                            .Intersect(before.Select(keyExpression.Compile()))
                                            .ToList();

            List<TPropertyType> beforeAmendedFields = before
                                                        .Where(entity => amendedKeys.Contains(keyExpression.Compile()(entity)))
                                                        .Select(fieldToTrackExpression.Compile())
                                                        .ToList();

            List<TPropertyType> afterAmendedFields = after
                                                        .Where(entity => amendedKeys.Contains(keyExpression.Compile()(entity)))
                                                        .Select(fieldToTrackExpression.Compile())
                                                        .ToList();

            List<Difference> amendmentDifferences = new List<Difference>();

            for (int i = 0; i < amendedKeys.Count; i++)
            {
                Difference amendmentDifference = new Difference()
                {
                    Type = TypeOfDifference.Amend,
                    PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{((MemberExpression)fieldToTrackExpression.Body).Member.Name}",
                    NewValue = afterAmendedFields[i].ToString(),
                    OldValue = beforeAmendedFields[i].ToString()
                };

                amendmentDifferences.Add(amendmentDifference);
            }

            if (_customAmendment == null)
                throw new ObjectTrackerException("Custom Amendment procedure not set.");

            return _customAmendment(amendmentDifferences);
        }


        public List<TResult> GetDifferences() => _differences;

    }

    public class ObjectTracker : ObjectTracker<Difference>
    {
        public ObjectTracker()
            
        {
            SetCustomAddition(CustomAdditionDeletionAmendment);
            SetCustomDeletion(CustomAdditionDeletionAmendment);
            SetCustomAmendment(CustomAdditionDeletionAmendment);
        }

        private List<Difference> CustomAdditionDeletionAmendment(List<Difference> differences) => differences;
    }
}
