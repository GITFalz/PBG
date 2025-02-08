using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class ModelingBase
{
    public abstract void Start(ModelingEditor editor);
    public abstract void Update(ModelingEditor editor);
    public abstract void Render(ModelingEditor editor);
    public abstract void Exit(ModelingEditor editor);
}