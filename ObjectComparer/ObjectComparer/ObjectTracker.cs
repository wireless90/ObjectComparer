using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
            :this(null, null)
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

        public ObjectTracker<TResult> Track<TObjectTypeToTrack, TKeyType, TPropertyType>(
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
                    PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{typeof(TPropertyType).Name}",
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
                    PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{typeof(TPropertyType).Name}",
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
                    PropertyName = $"{typeof(TObjectTypeToTrack).Name}.{typeof(TPropertyType).Name}",
                    NewValue = afterAmendedFields[i].ToString(),
                    OldValue = beforeAmendedFields[i].ToString()
                };

                amendmentDifferences.Add(amendmentDifference);
            }

            if (_customDeletion == null)
                throw new ObjectTrackerException("Custom Deletion procedure not set.");

            return _customDeletion(amendmentDifferences);
        }


        public List<TResult> GetDifferences() => _differences;

    }

    public class ObjectTracker : ObjectTracker<Difference>
    {
        public ObjectTracker()
            
        {
            SetCustomAddition(CustomAdditionDeletionAmendment);
            SetCustomDeletion(CustomAdditionDeletionAmendment);
        }

        private List<Difference> CustomAdditionDeletionAmendment(List<Difference> differences) => differences;
    }
}
