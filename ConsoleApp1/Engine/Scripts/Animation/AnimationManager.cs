using OpenTK.Mathematics;

public class AnimationManager
{
    public static AnimationManager Instance;
    
    public AnimationManager()
    {
        Instance = this;
    }
    
    public Dictionary<string, AnimationController> AnimationControllers = new Dictionary<string, AnimationController>();

    public void Start()
    {
        // Player
        AnimationController playerController = new AnimationController("Player");
        AnimationControllers.Add("Player", playerController);
        
        Animation playerAttack1 = new Animation();
        playerController.Animations.Add("attack1", playerAttack1);
        
        playerAttack1.Keyframes.Add(new AnimationKeyframe(Vector3.One, (5, -10, 0), (0, 0, 0.1f)));
        playerAttack1.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (10, -10, 0), (0, 0, 0.1f)));
        playerAttack1.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (7, 0, 7), (0.1f, 0, 0.1f)));
        playerAttack1.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (7, 10, 7), (0.1f, 0, -0.1f)));
        playerAttack1.SetKeyframe(15, new AnimationKeyframe(Vector3.One, (7, 10, 7), (0.1f, 0, 0.1f)));
        playerAttack1.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (10, 0, 0), (0, 0, 0.1f)));
        playerAttack1.SetKeyframe(25, new AnimationKeyframe(Vector3.One, (5, -10, 0), (0, 0, 0.1f)));
        
        Animation playerAttack2 = new Animation();
        playerController.Animations.Add("attack2", playerAttack2);
        
        playerAttack2.Keyframes.Add(new AnimationKeyframe(Vector3.One, (7, 10, 7), (0.1f, 0, -0.1f)));
        playerAttack2.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (7, 10, 7), (0.1f, 0, -0.1f)));
        playerAttack2.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (-10, 0, 0), (0, 0, 0.1f)));
        playerAttack2.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (-10, -10, 0), (0, 0, 0.1f)));
        playerAttack2.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (0, 0, 0), (0, 0, 0)));
        
        Animation playerAttack3 = new Animation();
        playerController.Animations.Add("attack3", playerAttack3);
        
        playerAttack3.Keyframes.Add(new AnimationKeyframe(Vector3.One, (5, -10, 0), (0, 0, 0.1f)));
        playerAttack3.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (10, -10, 0), (0, 0, 0.1f)));
        playerAttack3.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (7, 0, 7), (0.1f, 0, 0.1f)));
        playerAttack3.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (7, 10, 7), (0.1f, 0, -0.1f)));
        playerAttack3.SetKeyframe(15, new AnimationKeyframe(Vector3.One, (7, 10, 7), (0.1f, 0, 0.1f)));
        playerAttack3.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (10, 0, 0), (0, 0, 0.1f)));
        playerAttack3.SetKeyframe(25, new AnimationKeyframe(Vector3.One, (5, -10, 0), (0, 0, 0.1f)));
        
        Animation playerAttack4 = new Animation();
        playerController.Animations.Add("attack4", playerAttack4);
        
        playerAttack4.Keyframes.Add(new AnimationKeyframe(Vector3.One, (0, 0, 0), (0, 0, 0)));
        playerAttack4.SetKeyframe(7, new AnimationKeyframe(Vector3.One, (20, 100, 40), (0, 0.3f, -0.5f)));
        playerAttack4.SetKeyframe(12, new AnimationKeyframe(Vector3.One, (30, 240, 10), (0, 0.4f, -0.5f)));
        playerAttack4.SetKeyframe(14, new AnimationKeyframe(Vector3.One, (0, 10, 10), (0, 0, 0)));
        playerAttack4.SetKeyframe(17, new AnimationKeyframe(Vector3.One, (0, 5, 15), (0, 0, 0)));
        playerAttack4.SetKeyframe(25, new AnimationKeyframe(Vector3.One, (0, 0, 0), (0, 0, 0)));
        
        Animation playerRunning = new Animation();
        playerController.Animations.Add("running", playerRunning);
        
        playerRunning.Keyframes.Add(new AnimationKeyframe(Vector3.One, (0, -10, 0), (0, 0, 0)));
        playerRunning.SetKeyframe(5, new AnimationKeyframe(Vector3.One, (0f, -10, 0), (0f, 0.1f, 0.1f)));
        playerRunning.SetKeyframe(10, new AnimationKeyframe(Vector3.One, (0, -10, 0), (0, 0, 0)));
        playerRunning.SetKeyframe(15, new AnimationKeyframe(Vector3.One, (0, -10, 0), (0, 0.1f, -0.1f)));
        playerRunning.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (0, -10, 0), (0, 0, 0)));
        
        Animation playerIdle = new Animation();
        playerController.Animations.Add("idle", playerIdle);
        
        playerIdle.Keyframes.Add(new AnimationKeyframe(Vector3.One, (0, 0, 0), (0, 0, 0)));
        playerIdle.SetKeyframe(15, new AnimationKeyframe(Vector3.One, (0, 0, 0), (0, 0.03f, 0f)));
        playerIdle.SetKeyframe(30, new AnimationKeyframe(Vector3.One, (0, 0, 0), (0, 0, 0)));
        
        
        //Sword
        AnimationController swordController = new AnimationController("Sword");
        AnimationControllers.Add("Sword", swordController);
        
        Vector3 testPos = new Vector3(0.5f, 0, -1);
        
        swordController.baseAnimation = new Animation();
        swordController.Animations.Add("idle", swordController.baseAnimation);
        
        swordController.baseAnimation.Keyframes.Add(new AnimationKeyframe(Vector3.One, (-90, 10, -87), (0.5f, 0, -1)));
        swordController.baseAnimation.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (-90, 10, -93), (0.5f, 0, -1)));
        swordController.baseAnimation.SetKeyframe(40, new AnimationKeyframe(Vector3.One, (-90, 10, -87), (0.5f, 0, -1)));
        
        swordController.Animations.Add("attack1", new Animation());
        Animation attack1 = swordController.Animations["attack1"];
        
        attack1.Keyframes.Add(new AnimationKeyframe(Vector3.One, (-90, 10, -90), testPos));
        attack1.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (-90, 0, -41), (-1, 0, -1)));
        attack1.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (-90, 10, 129), (-1, 0, 1)));
        attack1.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (-90, 20, 170), (-1, 0, 1)));
        attack1.SetKeyframe(15, new AnimationKeyframe(Vector3.One, (-90, 10, 129), (-1, 0, 1)));
        attack1.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (-90, 0, -41), (-1, 0, -1)));
        attack1.SetKeyframe(25, new AnimationKeyframe(Vector3.One, (-90, 10, -90), testPos));
        
        swordController.Animations.Add("attack2", new Animation());
        Animation attack2 = swordController.Animations["attack2"];
        
        attack2.Keyframes.Add(new AnimationKeyframe(Vector3.One, (-90, -10, 170), (-1, 0, 1)));
        attack2.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (-90, 0, 129), (-1, 0, 1)));
        attack2.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (-90, -10, -41), (-1, 0, -1)));
        attack2.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (-90, -20, -90), testPos));
        
        swordController.Animations.Add("attack3", new Animation());
        Animation attack3 = swordController.Animations["attack3"];
        
        attack3.Keyframes.Add(new AnimationKeyframe(Vector3.One, (-90, -40, -90), testPos));
        attack3.SetKeyframe(2, new AnimationKeyframe(Vector3.One, (-90, -30, -41), (-1, 0, -1)));
        attack3.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (-90, 0, 129), (-1, 0, 1)));
        attack3.SetKeyframe(8, new AnimationKeyframe(Vector3.One, (-90, -30, 170), (-1, 0, 1)));
        attack3.SetKeyframe(15, new AnimationKeyframe(Vector3.One, (-90, -20, 129), (-1, 0, 1)));
        attack3.SetKeyframe(20, new AnimationKeyframe(Vector3.One, (-90, -10, -41), (-1, 0, -1)));
        attack3.SetKeyframe(25, new AnimationKeyframe(Vector3.One, (-90, -10, -90), testPos));
        
        swordController.Animations.Add("attack4", new Animation());
        Animation attack4 = swordController.Animations["attack4"];

        attack4.Keyframes.Add(new AnimationKeyframe(Vector3.One, (-90, -30, 170), (-1, 0, 1)));
        attack4.SetKeyframe(6, new AnimationKeyframe(Vector3.One, (-90, -70, 170), (0.6f, 0.2f, 0.7f)));
        attack4.SetKeyframe(12, new AnimationKeyframe(Vector3.One, (-30, 0, 0), (1f, 0.7f, -0.4f)));
        attack4.SetKeyframe(14, new AnimationKeyframe(Vector3.One, (0, 20, 110), (-0.6f, 0f, -0.7f)));
        attack4.SetKeyframe(22, new AnimationKeyframe(Vector3.One, (0, 20, 110), (-0.6f, 0f, -0.7f)));
        attack4.SetKeyframe(28, new AnimationKeyframe(Vector3.One, (-45, -20, 40), testPos));
        attack4.SetKeyframe(35, new AnimationKeyframe(Vector3.One, (-90, -20, -90), testPos));
    }
    
    public void Update(float angle)
    {
        foreach (var controller in AnimationControllers)
        {
            controller.Value.Update(angle);
        }
    }
    
    public void SetAnimation(string controller, string animation)
    {
        if (AnimationControllers.TryGetValue(controller, out var animController))
        {
            animController.SetAnimation(animation);
            animController.loop = false;
        }
    }
    
    public void LoopAnimation(string controller, string animation)
    {
        if (AnimationControllers.TryGetValue(controller, out var animController))
        {
            animController.SetAnimation(animation);
            animController.loop = true;
        }
    }
    
    public bool SetMesh(string controller, AnimationMesh mesh)
    {
        if (AnimationControllers.TryGetValue(controller, out var animController))
        {
            animController.mesh = mesh;
            return true;
        }
        
        return false;
    }
    
    public bool GetController(string controller, out AnimationController? animController)
    {
        return AnimationControllers.TryGetValue(controller, out animController);
    }
}