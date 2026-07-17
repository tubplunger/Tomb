using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tomb.Core.Debugging;
using Tomb.Core.Events;
using Tomb.Core.Save;

namespace Tomb.Gameplay.Resources
{
    public sealed class ResourceSystem : ISaveable
    {
        private const float ComparisonTolerance = 0.0001f;

        private readonly EventBus eventBus;
        private readonly DebugLogger debugLogger;

        private readonly Dictionary<string, ResourceState>
            resourcesById = new();

        private readonly List<ResourceState>
            orderedResources = new();

        public string SaveKey => "station_resources";

        public Type SaveStateType =>
            typeof(ResourceSaveState);

        public IReadOnlyList<ResourceState> Resources =>
            orderedResources;

        public ResourceSystem(
            EventBus eventBus,
            DebugLogger debugLogger,
            ResourceCatalog catalog)
        {
            this.eventBus = eventBus;
            this.debugLogger = debugLogger;

            InitializeFromCatalog(catalog);
        }

        public bool TryGetResource(
            string resourceId,
            out ResourceState resource)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                resource = null;
                return false;
            }

            return resourcesById.TryGetValue(
                resourceId,
                out resource
            );
        }

        public ResourceState GetResource(string resourceId)
        {
            if (TryGetResource(resourceId, out ResourceState resource))
                return resource;

            throw new KeyNotFoundException(
                $"Resource not found: {resourceId}"
            );
        }

        public float GetAmount(string resourceId)
        {
            return GetResource(resourceId).CurrentAmount;
        }

        public bool HasAtLeast(
            string resourceId,
            float amount)
        {
            return TryGetResource(resourceId, out ResourceState resource) &&
                   resource.HasAtLeast(amount);
        }

        public float Add(
            string resourceId,
            float amount,
            string reason = "Unspecified")
        {
            if (!TryGetResource(resourceId, out ResourceState resource))
            {
                LogMissingResource(resourceId);
                return 0f;
            }

            if (amount <= 0f)
                return 0f;

            float previousAmount = resource.CurrentAmount;
            bool wasLow = resource.IsLow;
            bool wasEmpty = resource.IsEmpty;

            float amountAdded = resource.Add(amount);

            if (amountAdded <= ComparisonTolerance)
                return 0f;

            PublishChange(
                resource,
                previousAmount,
                amountAdded,
                reason
            );

            EvaluateThresholdChanges(
                resource,
                wasLow,
                wasEmpty
            );

            return amountAdded;
        }

        public float Consume(
            string resourceId,
            float amount,
            string reason = "Unspecified")
        {
            if (!TryGetResource(resourceId, out ResourceState resource))
            {
                LogMissingResource(resourceId);
                return 0f;
            }

            if (amount <= 0f)
                return 0f;

            float previousAmount = resource.CurrentAmount;
            bool wasLow = resource.IsLow;
            bool wasEmpty = resource.IsEmpty;

            float amountConsumed = resource.Consume(amount);

            if (amountConsumed <= ComparisonTolerance)
                return 0f;

            PublishChange(
                resource,
                previousAmount,
                -amountConsumed,
                reason
            );

            EvaluateThresholdChanges(
                resource,
                wasLow,
                wasEmpty
            );

            return amountConsumed;
        }

        public bool TryConsumeExact(
            string resourceId,
            float amount,
            string reason = "Unspecified")
        {
            if (amount <= 0f)
                return true;

            if (!TryGetResource(resourceId, out ResourceState resource))
            {
                LogMissingResource(resourceId);
                return false;
            }

            if (!resource.HasAtLeast(amount))
                return false;

            Consume(resourceId, amount, reason);
            return true;
        }

        public float SetAmount(
            string resourceId,
            float amount,
            string reason = "Unspecified")
        {
            if (!TryGetResource(resourceId, out ResourceState resource))
            {
                LogMissingResource(resourceId);
                return 0f;
            }

            float previousAmount = resource.CurrentAmount;
            bool wasLow = resource.IsLow;
            bool wasEmpty = resource.IsEmpty;

            float changeAmount = resource.SetAmount(amount);

            if (Math.Abs(changeAmount) <= ComparisonTolerance)
                return 0f;

            PublishChange(
                resource,
                previousAmount,
                changeAmount,
                reason
            );

            EvaluateThresholdChanges(
                resource,
                wasLow,
                wasEmpty
            );

            return changeAmount;
        }

        public bool HasAll(
    IReadOnlyList<ResourceAmount> requirements)
        {
            if (requirements == null)
                return true;

            foreach (ResourceAmount requirement in requirements)
            {
                if (requirement == null ||
                    requirement.Resource == null ||
                    requirement.Amount <= 0f)
                {
                    continue;
                }

                if (!HasAtLeast(
                        requirement.ResourceId,
                        requirement.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryConsumeAll(
            IReadOnlyList<ResourceAmount> requirements,
            string reason = "Unspecified")
        {
            if (!HasAll(requirements))
                return false;

            if (requirements == null)
                return true;

            foreach (ResourceAmount requirement in requirements)
            {
                if (requirement == null ||
                    requirement.Resource == null ||
                    requirement.Amount <= 0f)
                {
                    continue;
                }

                Consume(
                    requirement.ResourceId,
                    requirement.Amount,
                    reason
                );
            }

            return true;
        }

        public void AddAll(
            IReadOnlyList<ResourceAmount> outputs,
            float multiplier = 1f,
            string reason = "Unspecified")
        {
            if (outputs == null || multiplier <= 0f)
                return;

            foreach (ResourceAmount output in outputs)
            {
                if (output == null ||
                    output.Resource == null ||
                    output.Amount <= 0f)
                {
                    continue;
                }

                Add(
                    output.ResourceId,
                    output.Amount * multiplier,
                    reason
                );
            }
        }

        public object CaptureState()
        {
            ResourceSaveState saveState =
                new ResourceSaveState();

            foreach (ResourceState resource in orderedResources)
            {
                saveState.resources.Add(
                    new ResourceSaveEntry
                    {
                        resourceId =
                            resource.Definition.ResourceId,

                        currentAmount =
                            resource.CurrentAmount
                    }
                );
            }

            return saveState;
        }

        public void RestoreState(object state)
        {
            if (state is not ResourceSaveState saveState)
                return;

            int restoredCount = 0;

            foreach (ResourceSaveEntry entry in saveState.resources)
            {
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.resourceId))
                {
                    continue;
                }

                if (!TryGetResource(
                        entry.resourceId,
                        out ResourceState resource))
                {
                    debugLogger.Log(
                        $"Save contains unknown resource: " +
                        $"{entry.resourceId}",
                        "Resources"
                    );

                    continue;
                }

                SetAmount(
                    entry.resourceId,
                    entry.currentAmount,
                    "Save restoration"
                );

                restoredCount++;
            }

            eventBus.Publish(
                new ResourceRestoredFromSaveEvent(restoredCount)
            );

            debugLogger.Log(
                $"Restored {restoredCount} resource values.",
                "Resources"
            );
        }

        private void InitializeFromCatalog(
            ResourceCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(
                    nameof(catalog),
                    "Resource catalog cannot be null."
                );
            }

            foreach (ResourceDefinition definition in catalog.Resources)
            {
                RegisterDefinition(definition);
            }

            debugLogger.Log(
                $"Resource system initialized with " +
                $"{orderedResources.Count} resources.",
                "Resources"
            );
        }

        private void RegisterDefinition(
            ResourceDefinition definition)
        {
            if (definition == null)
            {
                debugLogger.Log(
                    "Resource catalog contains a null entry.",
                    "Resources"
                );

                return;
            }

            string resourceId =
                definition.ResourceId;

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                debugLogger.Log(
                    $"Resource definition '{definition.name}' " +
                    "has an empty Resource ID.",
                    "Resources"
                );

                return;
            }

            if (resourcesById.ContainsKey(resourceId))
            {
                debugLogger.Log(
                    $"Duplicate resource ID rejected: {resourceId}",
                    "Resources"
                );

                return;
            }

            ResourceState state =
                new ResourceState(definition);

            resourcesById.Add(resourceId, state);
            orderedResources.Add(state);
        }

        private void PublishChange(
            ResourceState resource,
            float previousAmount,
            float changeAmount,
            string reason)
        {
            eventBus.Publish(
                new ResourceChangedEvent(
                    resource.Definition.ResourceId,
                    previousAmount,
                    resource.CurrentAmount,
                    resource.MaximumAmount,
                    changeAmount,
                    reason
                )
            );
        }

        private void EvaluateThresholdChanges(
            ResourceState resource,
            bool wasLow,
            bool wasEmpty)
        {
            if (!wasEmpty && resource.IsEmpty)
            {
                eventBus.Publish(
                    new ResourceEmptyEvent(
                        resource.Definition.ResourceId
                    )
                );

                return;
            }

            if (!wasLow && !wasEmpty && resource.IsLow)
            {
                eventBus.Publish(
                    new ResourceLowEvent(
                        resource.Definition.ResourceId,
                        resource.CurrentAmount,
                        resource.MaximumAmount
                    )
                );

                return;
            }

            if ((wasLow || wasEmpty) &&
                !resource.IsLow &&
                !resource.IsEmpty)
            {
                eventBus.Publish(
                    new ResourceRecoveredEvent(
                        resource.Definition.ResourceId,
                        resource.CurrentAmount,
                        resource.MaximumAmount
                    )
                );
            }
        }

        private void LogMissingResource(string resourceId)
        {
            debugLogger.Log(
                $"Unknown resource requested: {resourceId}",
                "Resources"
            );
        }
    }
}