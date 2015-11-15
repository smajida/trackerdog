﻿namespace TrackerDog
{
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    [ContractClassFor(typeof(IChangeTrackableObject))]
    internal abstract class IChangeTrackableObjectContract : IChangeTrackableObject
    {
        public ObjectChangeTracker ChangeTracker { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(IChangeTrackableObject trackableObject, string propertyName)
        {
            Contract.Requires(trackableObject != null, "A non null reference to a trackable object is mandatory");
            Contract.Requires(!string.IsNullOrEmpty(propertyName), "Changed property must have a name");
            Contract.Assert(PropertyChanged != null, "This event requires at least an event handler to be raised");
        }

        public void StartTracking(IChangeTrackableObject trackableObject, ObjectChangeTracker currentTracker = null)
        {
            Contract.Requires(trackableObject != null, "Given reference must be non-null to be able to track it");
        }

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(ChangeTracker != null, "A trackable object must own a change tracker");
        }
    }
}