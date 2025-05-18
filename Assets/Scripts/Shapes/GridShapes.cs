using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridShapeO : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.O;

    public GridShapeO() : this(new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
    })
    {

    }

    protected GridShapeO(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeI : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.I;

    public GridShapeI() : this(new Vector2Int[]
    {
        new Vector2Int(0, 2),
        new Vector2Int(0, 1),
        new Vector2Int(0, 0),
        new Vector2Int(0, -1),
    })
    {

    }

    protected GridShapeI(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeS : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.S;

    public GridShapeS() : this(new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(0, 0),
        new Vector2Int(-1, 0),
    })
    {

    }

    protected GridShapeS(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeZ : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.Z;

    public GridShapeZ() : this(new Vector2Int[]
    {
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
    })
    {

    }

    protected GridShapeZ(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeL : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.L;

    public GridShapeL() : this(new Vector2Int[]
    {
        new Vector2Int(0, 2),
        new Vector2Int(0, 1),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
    })
    {

    }

    protected GridShapeL(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeJ : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.J;

    public GridShapeJ() : this(new Vector2Int[]
    {
        new Vector2Int(0, 2),
        new Vector2Int(0, 1),
        new Vector2Int(0, 0),
        new Vector2Int(-1, 0),
    })
    {

    }

    protected GridShapeJ(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeT : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.T;

    public GridShapeT() : this(new Vector2Int[]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
    })
    {

    }

    protected GridShapeT(Vector2Int[] points) : base(points)
    {

    }
}

public class GridShapeSingle : GridShape
{
    public override GridShapeType GridShapeType => GridShapeType.Single;

    public GridShapeSingle() : this(new Vector2Int[]
    {
        new Vector2Int(0, 0)
    })
    {

    }

    protected GridShapeSingle(Vector2Int[] points) : base(points)
    {

    }
}