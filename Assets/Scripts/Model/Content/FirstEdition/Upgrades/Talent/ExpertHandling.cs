﻿using Upgrade;
using System.Collections.Generic;
using ActionsList;
using Ship;
using System.Linq;
using Tokens;
using SubPhases;

namespace UpgradesList.FirstEdition
{
    public class ExpertHandling : GenericUpgrade
    {
        public ExpertHandling() : base()
        {
            UpgradeInfo = new UpgradeCardInfo(
                "Expert Handling",
                UpgradeType.Talent,
                cost: 2,
                abilityType: typeof(Abilities.FirstEdition.ExpertHandlingAbility)
            );
        }        
    }
}

namespace Abilities.FirstEdition
{
    public class ExpertHandlingAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnGenerateActions += AddExpertHandlingAction;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnGenerateActions -= AddExpertHandlingAction;
        }

        private void AddExpertHandlingAction(GenericShip host)
        {
            GenericAction newAction = new ExpertHandlingAction()
            {
                ImageUrl = HostUpgrade.ImageUrl,
                HostShip = host,
                Source = HostUpgrade
            };
            host.AddAvailableAction(newAction);
        }
    }
}

namespace ActionsList
{

    public class ExpertHandlingAction : GenericAction
    {
        public ExpertHandlingAction()
        {
            Name = DiceModificationName = "Expert Handling";
        }

        public override void ActionTake()
        {
            Phases.CurrentSubPhase.Pause();
            if (!Selection.ThisShip.IsAlreadyExecutedAction(new BarrelRollAction()))
            {
                Phases.StartTemporarySubPhaseOld(
                    "Expert Handling: Barrel Roll",
                    typeof(BarrelRollPlanningSubPhase),
                    CheckStress
                );
            }
            else
            {
                Messages.ShowError(Selection.ThisShip.PilotInfo.PilotName + " cannot use Expert Handling: this ship has already executed a Barrel Roll action this round");
                Phases.CurrentSubPhase.Resume();
            }
        }

        private void CheckStress()
        {
            Selection.ThisShip.AddAlreadyExecutedAction(new BarrelRollAction());

            bool hasBarrelRollAction = HostShip.ActionBar.HasAction(typeof(BarrelRollAction));

            if (hasBarrelRollAction)
            {
                RemoveTargetLock();
            }
            else
            {
                HostShip.Tokens.AssignToken(typeof(StressToken), RemoveTargetLock);
            }

        }

        private void RemoveTargetLock()
        {
            if (HostShip.Tokens.HasToken(typeof(RedTargetLockToken), '*'))
            {
                ExpertHandlingTargetLockDecisionSubPhase subphase = Phases.StartTemporarySubPhaseNew<ExpertHandlingTargetLockDecisionSubPhase>(
                    "Expert Handling: Select target lock to remove",
                    Finish
                );

                subphase.DescriptionShort = "Expert Handling";
                subphase.DescriptionLong = "Select a target lock to remove";
                subphase.ImageSource = Source;

                subphase.Start();
            }
            else
            {
                Finish();
            }
        }

        private void Finish()
        {
            Phases.CurrentSubPhase.CallBack();
        }

    }

}

namespace SubPhases
{

    public class ExpertHandlingTargetLockDecisionSubPhase : DecisionSubPhase
    {

        public override void PrepareDecision(System.Action callBack)
        {
            foreach (var token in Selection.ThisShip.Tokens.GetAllTokens())
            {
                if (token.GetType() == typeof(RedTargetLockToken))
                {
                    AddDecision(
                        "Remove token \"" + (token as RedTargetLockToken).Letter + "\"",
                        delegate { RemoveRedTargetLockToken((token as RedTargetLockToken).Letter); }
                    );
                }
            }

            AddDecision("Don't remove", delegate { ConfirmDecision(); });

            DefaultDecisionName = GetDecisions().First().Name;

            callBack();
        }

        private void RemoveRedTargetLockToken(char letter)
        {
            Selection.ThisShip.Tokens.RemoveToken(
                typeof(RedTargetLockToken),
                ConfirmDecision,
                letter
            );
        }

    }

}