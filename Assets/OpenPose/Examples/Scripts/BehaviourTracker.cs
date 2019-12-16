using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BehaviourTracker : MonoBehaviour
{
    private GameObject clintonBox;
    private Text armsCrossedText;
    private Text handsInsideText;

    public void ArmsCrossed()
    {
        if (GameObject.Find("3_RElbow") && GameObject.Find("4_RWrist") && GameObject.Find("6_LElbow") && GameObject.Find("7_LWrist"))
        {
            // Get in between shoulders distance for scale reference
            Vector2 rShoulder = GameObject.Find("2_RShoulder").GetComponent<RectTransform>().localPosition;
            Vector2 lShoulder = GameObject.Find("5_LShoulder").GetComponent<RectTransform>().localPosition;
            float shoulderDistance = Vector2.Distance(rShoulder, lShoulder);

            // We want to check if each wrist overlaps with the opposite elbow
            Vector2 rElbow = GameObject.Find("3_RElbow").GetComponent<RectTransform>().localPosition;
            Vector2 rWrist = GameObject.Find("4_RWrist").GetComponent<RectTransform>().localPosition;
            Vector2 lElbow = GameObject.Find("6_LElbow").GetComponent<RectTransform>().localPosition;
            Vector2 lWrist = GameObject.Find("7_LWrist").GetComponent<RectTransform>().localPosition;

            float rElbowlWristDistance = Vector2.Distance(rElbow, lWrist);
            float lElbowrWristDistance = Vector2.Distance(lElbow, rWrist);

            // 0.7 * shoulderDistance seems like a reasonable choice for wrist/elbow limit distance
            if (rElbowlWristDistance < 0.7 * shoulderDistance && lElbowrWristDistance < 0.7 * shoulderDistance)
            {
                armsCrossedText.text = "Arms crossed";
            }
            else
            {
                armsCrossedText.text = "Arms not crossed";
            }
        }
    }

    // Clinton box is a virtual box ranging from both shoulders to both hips in which hands must remain
    public void UpdateClintonBox()
    {
        if (clintonBox)
        {
            if (GameObject.Find("9_RHip") && GameObject.Find("12_LHip"))
            {
                // auto-convert from Vector3 to Vector2 (z is discarded)
                Vector2 rShoulder = GameObject.Find("2_RShoulder").GetComponent<RectTransform>().localPosition;
                Vector2 lShoulder = GameObject.Find("5_LShoulder").GetComponent<RectTransform>().localPosition;
                Vector2 rHip = GameObject.Find("9_RHip").GetComponent<RectTransform>().localPosition;
                Vector2 lHip = GameObject.Find("12_LHip").GetComponent<RectTransform>().localPosition;

                // For scaling purpose
                float scaleFactor = Vector2.Distance(rShoulder, lShoulder) / 3;

                // Enlarge box
                rHip.x = rShoulder.x - scaleFactor;
                lHip.x = lShoulder.x + scaleFactor;
                rShoulder.x -= scaleFactor;
                lShoulder.x += scaleFactor;

                Vector2[] points = new Vector2[]
                {
                    rShoulder,
                    lShoulder,
                    lHip,
                    rHip
                };

                clintonBox.SetActive(true);
                clintonBox.GetComponent<ClintonBox>().UpdateMesh(points);

                HandsInClintonBox();
            }
            else
            {
                clintonBox.SetActive(false);
            }
        }
    }    

    private void HandsInClintonBox()
    {
        // Tag is on 9_Middle1 object on both hands of the Human2D prefab
        GameObject[] hands = GameObject.FindGameObjectsWithTag("Hand");

        if (hands.Length == 2)
        {
            // Actually not sure if 0 is left hand and 1 is right or the other way but it shouldn't matter
            Vector3 lHand = hands[0].GetComponent<RectTransform>().localPosition;
            Vector3 rHand = hands[1].GetComponent<RectTransform>().localPosition;

            if (clintonBox.GetComponent<ClintonBox>().Contains(lHand) && clintonBox.GetComponent<ClintonBox>().Contains(rHand))
            {
                handsInsideText.text = "Hands inside";
            }
            else
            {
                handsInsideText.text = "Hands not inside";
            }
        }
    }

    void Start()
    {
        clintonBox = GameObject.Find("Clinton Box");
        armsCrossedText = GameObject.Find("ArmsCrossed").GetComponent<Text>();
        handsInsideText = GameObject.Find("HandsInside").GetComponent<Text>();
    }

    void Update()
    {

    }
}
