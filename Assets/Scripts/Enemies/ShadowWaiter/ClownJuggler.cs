using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClownJuggler : MonoBehaviour
{
    [Header("Juggling Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform[] jugglePoints;
    [SerializeField] private float juggleSpeed = 1.5f;   // time a hop takes
    [SerializeField] private float arcHeight = 1.0f;   // max height of the parabola

    private readonly List<GameObject> balls = new();

    private void Start()
    {
        // spawn one ball at each point and give it its own looping coroutine
        for (int i = 0; i < jugglePoints.Length; i++)
        {
            GameObject ball = Instantiate(ballPrefab, jugglePoints[i].position, Quaternion.identity);
            balls.Add(ball);

            // start the “grand tour”: this ball begins at index i, so its next target is i+1
            StartCoroutine(BallTour(ball, i));
        }
    }

    /// <summary>
    /// Infinite coroutine that makes a single ball hop from point to point in a loop.
    /// </summary>
    private IEnumerator BallTour(GameObject ball, int currentIndex)
    {
        int pointCount = jugglePoints.Length;

        while (true)
        {
            int nextIndex = (currentIndex + 1) % pointCount;
            Vector3 target = jugglePoints[nextIndex].position;

            // Hop over to the next point
            yield return MoveBall(ball, target);

            // Prepare for the next hop
            currentIndex = nextIndex;
        }
    }

    /// <summary>
    /// Moves one ball to a target position along a nice arc, then returns.
    /// </summary>
    private IEnumerator MoveBall(GameObject ball, Vector3 targetPosition)
    {
        float elapsed = 0f;
        Vector3 start = ball.transform.position;

        while (elapsed < juggleSpeed)
        {
            float t = elapsed / juggleSpeed;

            // horizontal interpolation
            Vector3 position = Vector3.Lerp(start, targetPosition, t);

            // vertical parabola y += 4h·t·(1-t)
            position.y += 4f * arcHeight * t * (1f - t);

            ball.transform.position = position;

            ball.transform.rotation = Quaternion.Euler(0, -90, 90);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ball.transform.position = targetPosition; // snap exactly to point
    }
}



//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ClownJuggler : MonoBehaviour
//{
//    [Header("Juggling Settings")]
//    [SerializeField] private GameObject ballPrefab;
//    [SerializeField] private Transform[] jugglePoints;
//    [SerializeField] private float juggleSpeed = 1.5f;
//    [SerializeField] private float arcHeight = 1.0f;

//    [Header("Throwing Settings")]
//    [SerializeField] private Transform throwPoint;
//    [SerializeField] private float throwForce = 12f;
//    [SerializeField] private float throwInterval = 3f;
//    [SerializeField] private float aimOffset = 1.2f;

//    private Transform player;
//    private List<GameObject> juggledBalls = new List<GameObject>();
//    private int nextThrowIndex = 0;

//    private void Start()
//    {
//        player = GameObject.FindGameObjectWithTag("Player")?.transform;

//        // Create the juggled balls
//        foreach (Transform point in jugglePoints)
//        {
//            GameObject newBall = Instantiate(ballPrefab, point.position, Quaternion.identity);
//            ChangeBallColor(newBall);  // Assign a random color
//            juggledBalls.Add(newBall);
//        }

//        // Start the juggling and throwing routines
//        StartCoroutine(JuggleBallsRoutine());
//        StartCoroutine(ThrowBallRoutine());
//    }

//    private IEnumerator JuggleBallsRoutine()
//    {
//        while (true)
//        {
//            for (int i = 0; i < juggledBalls.Count; i++)
//            {
//                if (juggledBalls[i] != null)
//                {
//                    int nextPoint = (i + 1) % jugglePoints.Length;
//                    StartCoroutine(MoveBall(juggledBalls[i], jugglePoints[nextPoint].position));
//                }
//            }
//            yield return new WaitForSeconds(juggleSpeed);
//        }
//    }

//    private IEnumerator MoveBall(GameObject ball, Vector3 targetPosition)
//    {
//        float elapsedTime = 0;
//        Vector3 startPosition = ball.transform.position;

//        while (elapsedTime < juggleSpeed)
//        {
//            float t = elapsedTime / juggleSpeed;

//            // Horizontal linear movement
//            Vector3 horizontal = Vector3.Lerp(startPosition, targetPosition, t);

//            // Add vertical arc (parabola)
//            float arc = 4 * arcHeight * t * (1 - t);
//            horizontal.y += arc;

//            ball.transform.position = horizontal;

//            // Optional: rotate ball for visual effect
//            ball.transform.Rotate(Vector3.right * 360 * Time.deltaTime);

//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }

//        ball.transform.position = targetPosition;
//    }

//    private IEnumerator ThrowBallRoutine()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(throwInterval);
//            ThrowBall();
//        }
//    }

//    private void ThrowBall()
//    {
//        if (juggledBalls.Count == 0 || player == null) return;

//        GameObject ballToThrow = juggledBalls[nextThrowIndex];

//        if (ballToThrow != null)
//        {
//            Rigidbody rb = ballToThrow.GetComponent<Rigidbody>();

//            if (rb != null)
//            {
//                // Remove from clown’s transform
//                ballToThrow.transform.parent = null;

//                // Apply aim variation
//                Vector3 targetPos = player.position + new Vector3(
//                    Random.Range(-aimOffset, aimOffset),
//                    0,
//                    Random.Range(-aimOffset, aimOffset)
//                );

//                // Launch direction
//                Vector3 direction = (targetPos - throwPoint.position).normalized;
//                rb.linearVelocity = direction * throwForce;
//            }
//        }

//        // Replace after throw
//        StartCoroutine(ReplaceJuggledBall(nextThrowIndex));

//        nextThrowIndex = (nextThrowIndex + 1) % juggledBalls.Count;
//    }

//    private IEnumerator ReplaceJuggledBall(int index)
//    {
//        yield return new WaitForSeconds(2f);
//        GameObject newBall = Instantiate(ballPrefab, jugglePoints[index].position, Quaternion.identity);
//        ChangeBallColor(newBall);
//        juggledBalls[index] = newBall;
//    }

//    private void ChangeBallColor(GameObject ball)
//    {
//        Renderer renderer = ball.GetComponent<Renderer>();
//        if (renderer != null)
//        {
//            renderer.material.color = new Color(Random.value, Random.value, Random.value);
//        }
//    }
//}
