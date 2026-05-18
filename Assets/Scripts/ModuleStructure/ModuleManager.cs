using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using System.Collections.Generic;

/** 
Central driver class to control the flow of logic on which Modules/sections/timelines/activities should play, and in sequence.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class ModuleManager : MonoBehaviour
{
    /**
    Modules are the collections of sections that form learning content within the application. They possess a name, and are made up of a collection of Sections.
    */
    [System.Serializable]
    public class Module
    {
        public string name; /**< The name of the Module. */
        public List<Section> sections; /**< A collection of Sections that make up a Module. */
    }

    /**
    Sections are combinations of timelines and activities that form a Module.
    */
    [System.Serializable]
    public class Section
    {
        public string name;
        public List<PlayableDirector> timelines; /**< A collection of timelines that make up a Section. Each timeline contains animations, subtitles and narration, and signals. */
        public List<ModuleActivities> activities; /**< A collection of activities that make up a Section. Note that for an activity to be paired with a timeline, it MUST be in the same index slot in this List as the relevant timeline in the timelines List (e.g. for an activity to play after the timeline in the [3] index of the timelines List, the activity must also be in the [3] slot of this List). */
    }

    [SerializeField] private ModuleActivityScheduler activityScheduler; /**< Reference to the ModuleActivityScheduler singleton. */
    public List<Module> modules = new List<Module>(); /**< A list of Modules that form the application as a whole. */

    private int currentModuleIndex = 0; /**< The current Module index. */
    private int currentSectionIndex = 0; /**< The current Section index. */
    private int currentTimelineIndex = 0; /**< The current timeline index. */
    private PlayableDirector activeDirector; /**< Reference to the current timeline. */

    private bool waitingForActivity = false; /**< Flag that determines if an activity is currently playing or not, used to yield moving to the next timeline in the Section. */
    
    /// Public property to access the currently active PlayableDirector.
    /// Used by systems that need to sync with the timeline (e.g., haptic feedback).
    public PlayableDirector GetActiveDirector => activeDirector;

    void Start()
    {

    }

    /**
    Plays the timeline in the currently-selected section.
    @return void
    */
    void PlayCurrentTimeline()
    {
        var module = modules[currentModuleIndex];
        var section = module.sections[currentSectionIndex];

        // If there are no timelines in the currently selected section, skip it
        if (section.timelines.Count == 0)
        {
            NextSection();
            return;
        }

        //uses my stoptimeline function
        StopTimeline();

        // Reassign current director, and play it
        activeDirector = section.timelines[currentTimelineIndex];
        activeDirector.stopped += OnTimelineFinished; // This is a subscription to an event! When the timeline finishes (the timeline being a PlayableDirector) it will automatically call OnTimelineFinished() below
        activeDirector.Play();
    }

    /**
    When a timeline finishes, this method is called to check for activities to determine what to play next.
    @param director Passed here to ensure that an unsubscription to the event is performed, mitigating event stacking.
    @return void
    */
    void OnTimelineFinished(PlayableDirector director)
    {
        if (waitingForActivity) return;

        Debug.Log($"Timeline finished: {director}");
        Debug.Log("waitingForActivity: " + waitingForActivity); //

        director.stopped -= OnTimelineFinished; // This is an unsubscription to an event! This is just to ensure that we don't end up with multiple stacking event listeners. It's good practice

        var section = modules[currentModuleIndex].sections[currentSectionIndex];

        // Check to see if there exists an activity for this timeline
        ModuleActivities activityForTimeline = null;

        if (section.activities != null && currentTimelineIndex < section.activities.Count)
        {
            activityForTimeline = section.activities[currentTimelineIndex];
            Debug.Log("activityForTimeline: " + activityForTimeline);
        }

        if (activityForTimeline != null && !waitingForActivity)
        {
            waitingForActivity = true;
            Debug.Log($"Activity has started: {activityForTimeline}");
            StartActivity(activityForTimeline);
            return;
        }

        // If we get here, there is no activity and we can skip directly to the next timeline
        waitingForActivity = false;
        NextTimeline();
    }

    /**
    Communicate with the ModuleActivityScheduler to initiate a passed-in activity.
    @param activity The activity to be commenced.
    @return void
    */
    void StartActivity(ModuleActivities activity)
    {
        // Skip if no activities are assigned
        if (activity == null)
        {
            OnActivityComplete();
            return;
        }

        // Talk to the activity scheduler and send the activity for this section
        activityScheduler.StartActivity(activity, this);
    }

    /**
    Called by the ModuleActivityScheduler when the current activity is completed, starting the next timeline.
    @return void
    */
    public void OnActivityComplete()
    {
        waitingForActivity = false;
        NextTimeline();
    }

    /**
    Progress to the next timeline, skipping instead to the next Section if there are no more timelines in this Section.
    @return void
    */
    void NextTimeline()
    {
        var section = modules[currentModuleIndex].sections[currentSectionIndex];
        currentTimelineIndex++;

        if (currentTimelineIndex >= section.timelines.Count)
        {
            NextSection();
            return;
        }

        PlayCurrentTimeline();
    }

    /**
    Progress to the next Section, skipping instead to the next Module if there are no more Sections in this Module.
    @return void
    */
    void NextSection()
    {
        currentTimelineIndex = 0;
        currentSectionIndex++;

        var module = modules[currentModuleIndex];
        if (currentSectionIndex >= module.sections.Count)
        {
            NextModule();
            return;
        }

        PlayCurrentTimeline();
    }

    /**
    Progress to the next Module. If there are no more Modules, we have reached the end of the learning content.
    @return void
    */
    void NextModule()
    {
        currentSectionIndex = 0;
        currentTimelineIndex = 0;
        currentModuleIndex++;

        if (currentModuleIndex >= modules.Count)
        {
            // TODO
            // Last module completed.
            return;
        }

        PlayCurrentTimeline();
    }

    //playing specific module and section
    public void PlayModuleSection(int moduleIndex, int sectionIndex)
    {
        waitingForActivity = false;
        //uses my stoptimeline function
        StopTimeline();

        //for checklists for each section to reset
        if (activityScheduler != null)
        {
            activityScheduler.ActivityReset();
        }
        //resets the indexes on moving to next section
        currentModuleIndex = moduleIndex;
        currentSectionIndex = sectionIndex;
        currentTimelineIndex = 0;

        PlayCurrentTimeline();
    }


    /*
    function used to stop timeline when selecting a section from menu (note for future developers activeDirector
    is a reference to the PlayableDirector in a timeline
    */
    private void StopTimeline()
    {
        if (activeDirector != null)
        {
            activeDirector.stopped -= OnTimelineFinished; // unsubscribe before Stop() to prevent spurious OnTimelineFinished call
            activeDirector.Stop(); //stops playback
            activeDirector.time = 0; //resets timeline

        }
    }

}
