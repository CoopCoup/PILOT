using UnityEngine;

public class TestingTimer : MonoBehaviour
{
    
    private void Start()
    {
        FunctionTimer.Create(TestingAction, 3f, "Timer");
        FunctionTimer.Create(TestingAction2, 4f, "Timer2");

        FunctionTimer.StopTimer("Timer");
    }

    private void TestingAction()
    {
        Debug.Log("Testing!");
    }

    private void TestingAction2()
    {
        Debug.Log("Testing 2!");
    }
}
