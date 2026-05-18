using UnityEngine;

/** 
Interface class. All activities implement this interface to standardize access from ModuleActivityScheduler's controller prefab instantiation.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public interface IActivityController
{
    void StartActivity(); /**< Each child activity can have a differing implementation of what must be carried out/what inputs must be switched on when the activity starts. Override this method for new activity scripts. */
    void StopActivity();  /**< Called when an activity is interrupted (e.g. section switch while paused). Stop any running coroutines and internal PlayableDirectors to prevent orphaned audio/animation. */
}
