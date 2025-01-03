﻿public abstract class Component
{
    public string name;
    
    public GameObject gameObject;
    public Transform transform;

    public virtual void OnResize()
    {
        
    }

    public virtual void Awake()
    {
        
    }
    
    public virtual void Start()
    {
        
    }
    
    public virtual void Update()
    {
        
    }
    
    public virtual void FixedUpdate()
    {

    }
    
    public virtual void Render()
    {

    }
    
    public virtual void Exit()
    {
        
    }
}