using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour
{
    public static FishController Instance;
    public Transform _pathtoMove;
    private bool _startingbool;
    public bool _startMousebool;
    private float startTime;
    public float speed = 1.0F;
    private float journeyLength;
    public Transform target;
    public Vector3 targetMouse;
    public Vector3 OrgtargetMouse;
    Quaternion targetRotation;
    public Animator _myAnimator;
   public  FishState myDirection;

    public enum FishState { Running, Walk };
     private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        myDirection = FishState.Walk;
        target = SelectRandomPoint();
        startTime = Time.time;
        _myAnimator.SetBool("ismoving", true);
        _myAnimator.SetBool("run", false);  
 
           journeyLength = Vector3.Distance(this.transform.position, target.position);
        _startingbool = true;
        Vector3 targetDirection = (target.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(targetDirection);
        _startMousebool = false; 
     }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (_startingbool) 
        {
             
            myDirection = FishState.Walk;
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;
            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;
            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(this.transform.position, target.position, fractionOfJourney);
             transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation,speed *10);
            if (Vector3.Distance(this.transform.position, target.position) < .1)  
            {  
                 target = SelectRandomPoint();
                startTime = Time.time;
                journeyLength = Vector3.Distance(this.transform.position, target.position);  
                _startingbool = true;
                 Vector3 targetDirection = (target.position - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(targetDirection);
            }  
         }
        else if (_startMousebool)
        {
            myDirection = FishState.Running;
             float distCovered = (Time.time - startTime) * speed;
             float fractionOfJourney = distCovered / journeyLength;
             transform.position = Vector3.Lerp(this.transform.position, targetMouse, fractionOfJourney *20); 
            transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, speed * 70);  
            if (Vector3.Distance(this.transform.position, targetMouse) < .05)
            {
                print("Reached here"); 
                RippleEffect.Instance.ReachedMousePoint(OrgtargetMouse);
                 _startingbool = false;
                _startMousebool = false;
                _myAnimator.SetBool("ismoving", false);
                _myAnimator.SetBool("run", false);
                _myAnimator.SetBool("idle", true);
             }  
        }
     }
    public void MouseTouchPoint(Vector3 _mousetouchPos)
    {  
        startTime = Time.time;
        OrgtargetMouse = _mousetouchPos;
        targetMouse = _mousetouchPos;
        targetMouse = new Vector3(targetMouse.x, targetMouse.y , targetMouse.z -.2f); 
         journeyLength = Vector3.Distance(this.transform.position, _mousetouchPos);
         Vector3 targetDirection = (targetMouse - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(targetDirection);
        _startingbool = false;
        _startMousebool = true;
        _myAnimator.SetBool("ismoving", false);
        _myAnimator.SetBool("run", true);      
    }   
    public void ReturntoPath()
    {
        print("returned to Path here");  

        target = SelectRandomPoint();
        startTime = Time.time;
        journeyLength = Vector3.Distance(this.transform.position, target.position);
        if(!_startMousebool)
        _startingbool = false; 
        //Vector3 targetDirection = (target.position - transform.position).normalized; 
        //targetRotation = Quaternion.LookRotation(targetDirection);
        _myAnimator.SetBool("ismoving", true);
        _myAnimator.SetBool("run", false);  
    }
    Transform  SelectRandomPoint()
    {
        int counter = _pathtoMove.childCount;
        int _SelectedChild =  Random.Range(0, counter);
        return _pathtoMove.GetChild(_SelectedChild);
    }  
}
