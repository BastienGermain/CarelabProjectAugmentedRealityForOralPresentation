// Addition to the original OpenPose plugin
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OscJack;

// Class to check postures and send data to OscReceiver

public class BehaviourTracker : MonoBehaviour
{
    public bool isArmsCrossed = false;
    public bool isHandsInside = false;
    public bool isShouldersAligned = true;

    private GameObject clintonBox;
    private Text armsCrossedText;
    private Text handsInsideText;
    private Text shouldersAlignedText;
    private Color black = new Color(0, 0, 0, 0.7f);
    private Color red = new Color(0.8f, 0, 0, 0.7f);

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
                armsCrossedText.color = red;
                isArmsCrossed = true;
                SendOSC(1, "/arms");
            }
            else
            {
                armsCrossedText.text = "Arms not crossed";
                armsCrossedText.color = black;
                isArmsCrossed = false;
                SendOSC(0, "/arms");
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

                // reduce box height at base
                rHip.y -= scaleFactor;
                lHip.y -= scaleFactor;

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
                handsInsideText.color = black;
                isHandsInside = true;
                SendOSC(1, "/hands");
            }
            else
            {
                handsInsideText.text = "Hands not inside";
                handsInsideText.color = red;
                isHandsInside = false;
                SendOSC(0, "/hands");
            }
        }
    }

    // Check if shoulders are aligned horizontally and nose, neck and middle hip vertically
    public void StraightPosture()
    {
        if (GameObject.Find("0_Nose") && GameObject.Find("1_Neck") && GameObject.Find("8_MidHip"))
        {
            Vector2 rShoulder = GameObject.Find("2_RShoulder").GetComponent<RectTransform>().localPosition;
            Vector2 lShoulder = GameObject.Find("5_LShoulder").GetComponent<RectTransform>().localPosition;
            Vector2 nose = GameObject.Find("0_Nose").GetComponent<RectTransform>().localPosition;
            Vector2 neck = GameObject.Find("1_Neck").GetComponent<RectTransform>().localPosition;
            Vector2 midHip = GameObject.Find("8_MidHip").GetComponent<RectTransform>().localPosition;

            float shoulderDistance = Vector2.Distance(rShoulder, lShoulder);

            if (Math.Abs(rShoulder.y - lShoulder.y) > shoulderDistance * 0.1) {
                shouldersAlignedText.text = "Shoulders not aligned";
                shouldersAlignedText.color = red;
                isShouldersAligned = false;
                SendOSC(0, "/shoulders");
            }
            else
            {
                shouldersAlignedText.text = "Shoulders aligned";
                shouldersAlignedText.color = black;
                isShouldersAligned = true;
                SendOSC(1, "/shoulders");
            }
        }        
    } 

    void Start()
    {
        clintonBox = GameObject.Find("Clinton Box");
        armsCrossedText = GameObject.Find("ArmsCrossed").GetComponent<Text>();
        handsInsideText = GameObject.Find("HandsInside").GetComponent<Text>();
        shouldersAlignedText = GameObject.Find("ShouldersAligned").GetComponent<Text>();
    }

    private void SendOSC(int value, string address)
    {
        // IP address, port number
        var client = new OscClient("163.221.174.235", 9100);

        client.Send(address, value);

        // Terminate the client.
        client.Dispose();
    }

}
