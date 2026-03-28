using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private CuttingCounter CuttingCounter;
    private const string CUT = "Cut";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        CuttingCounter.OnCut += CuttingCounter_OnCut;
    }

    private void CuttingCounter_OnCut(object sender, System.EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
