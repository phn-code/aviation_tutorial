using UnityEngine;

public class BankGhostTrailBehaviour : MonoBehaviour
{
    [Header("enable")]
    public bool enable = true;          /**< Enable banking behaviour*/
    [Header("references")]
    public ParticleSystem ghostTrail;   /**< Ghost trail particle system */
    [Header("Ghost Trail Motion")]
    public float velocity = 50f;        /**< Velocity of the particles */


    private ParticleSystem.Particle[] ghostParticles; /**< List of particles emitted by the particle system */

    //following are the angles that determine how the ghost trail behaves
    private bool isBanking = false; //variable to track whether it is banking (banking is the tilt of plane btw)
    public float bankDeadZone = 3f; //bank angle needed to start curving
    public float velocityBlendSpeed = 8f; //blend speed is made so that particles change direction slowly


    /**
    Main update loop

    Calls UpdateGhostTrailMotion() if enable flag set to true
    */
    void Update()
    {
        if (enable)
        {
            UpdateGhostTrailMotion();
        }
    }

    /** 
    Determines and updates behaviour of particles

    Creates a local copy of the assigned Particle System's particle buffer and modifies the tranform of particles.
    Particles follow the calculated radius and the logic is functional during level, ascending and descending flight.

    Formula for calculating turning radius
    \f[
    R = \frac{V^2}{g \times \tan(\theta)}
    \f]
    where:
    - \f$R\f$ = Radius (m)
    - \f$V\f$ = Velocity (m/s)
    - \f$g\f$ = Gravity (m/s²)
    - \f$\theta\f$ = Bank angle (degrees)

    Particles transforms are modified and buffer is returned to the Particle system.
    */
    void UpdateGhostTrailMotion()
    {
        if (ghostTrail == null) return;

        if (ghostParticles == null || ghostParticles.Length < ghostTrail.main.maxParticles)
        {
            ghostParticles = new ParticleSystem.Particle[ghostTrail.main.maxParticles];
        }

        int count = ghostTrail.GetParticles(ghostParticles);
        if (count == 0) return;

        float bankAngle = transform.rotation.eulerAngles.z; //change to z was x before
        // convert from unsigned 0-360 to range -180 and 180
        float signedBank = Mathf.DeltaAngle(0f, bankAngle);

        //added banking boolean to track when the plane is in banking position
        float absBank = Mathf.Abs(signedBank);
        if (!isBanking && absBank >= bankDeadZone) isBanking = true;
        else if (isBanking && absBank < bankDeadZone * 0.5f) isBanking = false;

        // turn direction -1 = right 1 = left
        float turnDir = Mathf.Sign(signedBank);

        // The planes forward axis is +x therefore transform.forward returns the direction the planes +z axis is facing
        Vector3 turnCentre = transform.position;

        /*using my boolean used previous group logic calculations to basically have the particles follow
         a curve when the plane is banking */
        if (isBanking)
        {
            // Mathf.Tan expects radians not degrees
            float tanBank = Mathf.Tan(absBank * Mathf.Deg2Rad);
            if (tanBank >= 0.01f)
            {
                /* Formula for calculating turning radius
                 * R = V^2 / (g * tan(BankAngle)
                 * R = Radius m
                 * V = velocity m/s
                 * g = gravity m/s
                 * bankAngle = degrees
                */
                // tighter bank = smaller radius = sharper curve
                // shallower bank = bigger radius = wider curve
                float radius = (velocity * velocity) / (9.81f * tanBank);
                // Isolate horizontal x and z directions
                Vector3 side = transform.forward;
                side.y = 0;
                // Normalize horizontal dir and change direction to left or right
                side = side.normalized * turnDir;
                // calculate turn centre
                turnCentre = transform.position + side * radius;
            }
        }

        float verticalComponent = -transform.right.y;

        // Update Each Ghost Particle
        for (int i = 0; i < count; i++)
        {
            Vector3 target;

            if (isBanking)
            {
                // get vector from current pos to centre
                Vector3 toCentre = turnCentre - ghostParticles[i].position;
                // remove y component
                toCentre.y = 0f;
                // normalize
                Vector3 dirToCentre = toCentre.normalized;

                // calculate the "tangent" using the cross product of the direction to centre and vector3.down
                // returns the a vector perpendicular to both inputs
                Vector3 tangentDir = Vector3.Cross(Vector3.down, dirToCentre).normalized;
                // flip the tangent direction based on the turn side
                tangentDir *= turnDir;

                // Add vertical component
                tangentDir.y = verticalComponent;

                // set the new velocity along the tangent
                target = tangentDir * velocity;
            }
            else
            {
                // return, particle system handles level flight by default
                target = ghostParticles[i].velocity;
            }

            // blend smoothly to target velocity instead of snapping, prevents flickering at origin
            ghostParticles[i].velocity = Vector3.Lerp(
                ghostParticles[i].velocity,
                target,
                Time.deltaTime * velocityBlendSpeed //uses delta time to gradually blend the particles as we change direction -randy
            );
        }

        // Pass the updated particles back to the ParticleSystem
        ghostTrail.SetParticles(ghostParticles, count);
    }

    /**
    Draw Gizmo Sphere representing turn radius

    Sphere represents the calculated radius of the plane and is rendered when GameObject is selected
    */
    void OnDrawGizmosSelected()
    {
        if (ghostTrail == null) return;

        // Get bank angle
        float signedBank = Mathf.DeltaAngle(0f, transform.rotation.eulerAngles.z);
        if (Mathf.Abs(signedBank) < 5f) return;

        // Mathf.Tan expects radians not degrees
        float tanBank = Mathf.Tan(Mathf.Abs(signedBank) * Mathf.Deg2Rad);
        if (tanBank < 0.01f) return;

        // Calculate radius
        float radius = (velocity * velocity) / (9.81f * tanBank);
        // The planes forward axis is +x therefore transform.forward returns the direction the planes +z axis is facing
        Vector3 side = transform.forward;
        // Isolate horizontal x and z directions
        side.y = 0;
        // change direction to left or right
        side = side.normalized * Mathf.Sign(signedBank);

        // cyan sphere in scene view shows the current turn radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + side * radius, radius);
    }
}