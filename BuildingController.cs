using System;

namespace TestDrivenAssignment
{
    public static class Constants
    {
        public const string Closed = @"closed";
        public const string OutOfHours = @"out of hours";
        public const string Open = @"open";
        public const string FireDrill = @"fire drill";
        public const string FireAlarm = @"fire alarm";
        public const string Default = OutOfHours;
        public const string EmailAddress = @"smartbuilding@uclan.ac.uk";
        public const string FireAlarmSubject = @"failed to log fire alarm";

    }

    public class BuildingController
    {
        private string buildingID { get; set; }   // The object's building ID variable.
        private string currentState { get; set; } // The building's current state.
        private string previousState { get; set; } // The building's previous state.
        public ILightManager LightManager { get; set; } // The building's Light Manager.
        public IFireAlarmManager FireAlarmManager { get; set; } // The building's Fire Alarm Manager.
        public IDoorManager DoorManager { get; set; } // The building's Door Manager.
        public IWebService WebService { get; set; } // The building's Web Service.
        public IEmailService EmailService { get; set; } // The building's Email Service.

        BuildingController() { }

        /// <summary>
        /// Create a new BuildingController object with an assigned Building ID
        /// and a default state of out of hours.
        /// </summary>
        /// <param name="id">The ID which the building will have.</param>
        public BuildingController(string id)
        {
            buildingID = id.ToLower();
            currentState = Constants.OutOfHours;
            previousState = Constants.OutOfHours;
        }

        /// <summary>
        /// Create a new BuildingController object with an assigned Building ID and State.
        /// </summary>
        /// <param name="id">The ID which the building will have.</param>
        /// <param name="startState">The state which the building will have.</param>
        public BuildingController(string id, string startState)
        {
            buildingID = id.ToLower();
            startState = startState.ToLower();
            if (startState == Constants.Closed || startState == Constants.OutOfHours || startState == Constants.Open)
            {
                currentState = startState;
                previousState = Constants.Default;
                return;
            }

            // Exception caused by not providing the correct starting state.
            // This is only reached if the start state provided does not fit the criteria.
            throw new ArgumentException(String.Format("Argument Exception: " +
                "BuildingController can only be initialised to the following states " +
                "'{0}', '{1}', '{2}'", Constants.Open, Constants.Closed, Constants.OutOfHours));
        }

        public BuildingController(string id, ILightManager iLightManager, IFireAlarmManager iFireAlarmManager,
            IDoorManager iDoorManager, IWebService iWebService, IEmailService iEmailService)
        {
            LightManager = iLightManager;
            FireAlarmManager = iFireAlarmManager;
            DoorManager = iDoorManager;
            WebService = iWebService;
            EmailService = iEmailService;
            buildingID = id.ToLower();
            currentState = Constants.Default;
            previousState = Constants.Default;
        }

        /// <summary>
        /// Return the value of the Building ID variable.
        /// </summary>
        /// <returns>The building's ID variable.</returns>
        public string GetBuildingID()
        {
            return buildingID;
        }

        /// <summary>
        /// Set the building ID variable to the parameter.
        /// </summary>
        /// <param name="id">The building's new ID.</param>
        public void SetBuildingID(string id)
        {
            buildingID = id.ToLower();
        }

        /// <summary>
        /// Return the value of the current state variable.
        /// </summary>
        /// <returns>Current state variable.</returns>
        public string GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// If the provided string is a valid state, the building's state is changed.
        /// Else the state remains unchanged.
        /// </summary>
        /// <param name="newState">The new state the building will change to.</param>
        /// <returns>True if state change succeeded. Else returns false.</returns>
        public bool SetCurrentState(string newState)
        {
            newState = newState.ToLower();
            if (currentState == newState)
            {
                return true;
            }

            // Fire Alarm and Fire Drill management
            if (previousState == newState && (currentState == Constants.FireAlarm || currentState == Constants.FireDrill))
            {
                if (newState == Constants.Open && !DoorManager.OpenAllDoors())
                {
                    return false;
                }
                previousState = currentState;
                currentState = newState;
                return true;
            }

            switch (newState)
            {
                case Constants.Closed:
                    {
                        if (currentState == Constants.OutOfHours)
                        {
                            previousState = currentState;
                            currentState = newState;

                            if (DoorManager != null)
                            {
                                DoorManager.LockAllDoors();
                            }
                            if (LightManager != null)
                            {
                                LightManager.SetAllLights(false);
                            }                       

                            return true;
                        }
                        return false;
                    }
                case Constants.OutOfHours:
                    {
                        if (currentState == Constants.Closed || currentState == Constants.Open)
                        {
                            previousState = currentState;
                            currentState = newState;
                            return true;
                        }
                        return false;
                    }
                case Constants.Open:
                    {
                        if (currentState == Constants.OutOfHours)
                        {
                            if (DoorManager != null)
                            {
                                if (!DoorManager.OpenAllDoors())
                                {
                                    return false;
                                }
                            }                            
                            previousState = currentState;
                            currentState = newState;
                            return true;
                        }
                        return false;
                    }
                case Constants.FireAlarm:
                    {
                        if (currentState != Constants.FireDrill)
                        {
                            previousState = currentState;
                            currentState = newState;

                            if (FireAlarmManager != null)
                            {
                                FireAlarmManager.SetAlarm(true);
                            }
                            
                            if (DoorManager != null)
                            {
                                DoorManager.OpenAllDoors();
                            }
                            
                            if (LightManager != null)
                            {
                                LightManager.SetAllLights(true);
                            }


                            if (WebService != null)
                            {
                                try
                                {
                                    WebService.LogFireAlarm("fire alarm");
                                }
                                catch (Exception e)
                                {
                                    EmailService.SendMail(Constants.EmailAddress, Constants.FireAlarmSubject, e.Message);
                                    return false;
                                }
                            }
                            return true;
                        }
                        return false;
                    }
                case Constants.FireDrill:
                    {
                        if (currentState != Constants.FireAlarm)
                        {
                            previousState = currentState;
                            currentState = newState;
                            return true;
                        }
                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public string GetStatusReport()
        {
            string output = LightManager.GetStatus() + DoorManager.GetStatus() + FireAlarmManager.GetStatus();
            return output;
        }

        static void Main(string[] args)
        {
            return;
        }
    }
}
