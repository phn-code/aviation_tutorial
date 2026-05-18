using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DA40 : Aircraft
{
    [Header("Landing Gear")]
    [SerializeField] private Aircraft_Part FrontLandingGear; // Fault index: 0
    [SerializeField] private Aircraft_Part RightLandingGear; // Fault index: 1
    [SerializeField] private Aircraft_Part LeftLandingGear; // Fault index: 2

    [Header("Steps")]
    [SerializeField] private Aircraft_Part RightStep; // Fault index: 3
    [SerializeField] private Aircraft_Part LeftStep; // Fault index:4

    [Header("Propeller")]
    [SerializeField] private Aircraft_Part Propeller; // Fault index: 5

    [Header("Fuselage")]
    [SerializeField] private Aircraft_Part Fuselage; // Fault index: 6

    [Header("Antenna")]
    [SerializeField] private Aircraft_Part Antenna; // Fault index: 7

    [Header("Wings")]
    [SerializeField] private Aircraft_Part RightWing; // Fault index: 8
    [SerializeField] private Aircraft_Part LeftWing; // Fault index: 9

    [Header("Ailerons")]
    [SerializeField] private Aircraft_Part RightAileron; // Fault index: 10
    [SerializeField] private Aircraft_Part LeftAileron; // Fault index: 11

    [Header("Break Flaps")]
    [SerializeField] private Aircraft_Part RightBreakFlap; // Fault index: 12
    [SerializeField] private Aircraft_Part LeftBreakFlap; // Fault index: 13

    [Header("Lift Flap")]
    [SerializeField] private Aircraft_Part LiftFlap; // Fault index: 14

    [Header("Rudder")]
    [SerializeField] private Aircraft_Part Rudder; // Fault index: 15

    [Header("Landing Light")]
    [SerializeField] private Aircraft_Part LandingLight; // Fault index: 16

    [Header("Traffic Lights")]
    [SerializeField] private Aircraft_Part RightTrafficLight; // Fault index: 17
    [SerializeField] private Aircraft_Part LeftTrafficLight; // Fault index: 18

    [Header("Strobe Lights")]
    [SerializeField] private Aircraft_Part RightStrobeLight; // Fault index: 19
    [SerializeField] private Aircraft_Part LeftStrobeLight; // Fault index: 20

    [Header("Stall Warning")]
    [SerializeField] private Aircraft_Part StallWarning; // Fault index: 21

    [Header("Pitot")]
    [SerializeField] private Aircraft_Part Pitot; // Fault index: 22

    [Header("Fuel Caps")]
    [SerializeField] private Aircraft_Part FuselageFuelCap; // Fault index: 23
    [SerializeField] private Aircraft_Part WingFuelCap; // Fault index: 24



    // Start is called before the first frame update.
    void Start()
    {
        parts = new Aircraft_Part[23];
        parts[0] = FrontLandingGear;
        parts[1] = RightLandingGear;
        parts[2] = LeftLandingGear;
        parts[3] = RightStep;
        parts[4] = LeftStep;
        parts[5] = Propeller;
        parts[6] = Fuselage;
        parts[7] = Antenna;
        parts[8] = RightWing;
        parts[9] = LeftWing;
        parts[10] = RightAileron;
        parts[11] = LeftAileron;
        parts[12] = RightBreakFlap;
        parts[13] = LeftBreakFlap;
        parts[14] = LiftFlap;
        parts[15] = Rudder;
        parts[16] = LandingLight;
        parts[17] = RightTrafficLight;
        parts[18] = LeftTrafficLight;
        parts[19] = RightStrobeLight;
        parts[20] = LeftStrobeLight;
        parts[21] = StallWarning;
        parts[22] = Pitot;
        // parts[23] = FuselageFuelCap;
        // parts[24] = WingFuelCap;
    }
}
