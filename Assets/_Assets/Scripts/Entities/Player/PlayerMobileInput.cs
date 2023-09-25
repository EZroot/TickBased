using System;
using UnityEngine;

public class PlayerMobileInput
{
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    public void UpdateInputDetection(Action<SwipDirection> OnSwipped)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Ended:
                    touchEndPos = touch.position;
                    var swipeDir = DetectSwipe();
                    OnSwipped?.Invoke(swipeDir);
                    break;
            }
        }
    }

    SwipDirection DetectSwipe()
    {
        Vector2 swipeVector = touchEndPos - touchStartPos;
        SwipDirection dir = SwipDirection.Down;
        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            // Horizontal swipe
            if (swipeVector.x > 0)
            {
                dir = SwipDirection.Right;
            }
            else
            {
                dir = SwipDirection.Left;
            }
        }
        else
        {
            // Vertical swipe
            if (swipeVector.y > 0)
            {
                dir = SwipDirection.Up;
            }
            else
            {
                dir = SwipDirection.Down;
            }
        }

        return dir;
    }

    public enum SwipDirection
    {
        Up,
        Down,
        Left,
        Right
    }
}