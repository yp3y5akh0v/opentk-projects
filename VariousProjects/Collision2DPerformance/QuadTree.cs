using System.Collections.Generic;
using System.Linq;
using OpenTK;
using SharedLib;

namespace Collision2DPerformance
{
    public class QuadTree
    {
        protected Rect2D range { get; set; }
        protected List<Circle2D> data { get; set; }
        protected Dictionary<QuadTreeDirection, QuadTree> childs { get; set; }
        protected QuadTree parent;
        protected QuadTreeDirection currDir;
        protected int capacity { get; set; }

        public enum QuadTreeDirection
        {
            Root,
            LeftBottom,
            LeftUp,
            RightUp,
            RightBottom
        }

        public QuadTree(Rect2D range, int capacity)
        {
            this.range = range;
            this.capacity = capacity;

            data = new List<Circle2D>();
            childs = new Dictionary<QuadTreeDirection, QuadTree>();
            currDir = QuadTreeDirection.Root;
        }

        public void Subdivide(QuadTreeDirection qtd)
        {
            var center = range.GetCenter();
            var halfsize = range.GetHalfSize();

            var step = Vector2.Zero;
            switch (qtd)
            {
                case QuadTreeDirection.LeftBottom:
                    step = new Vector2(-1, -1);
                    break;
                case QuadTreeDirection.LeftUp:
                    step = new Vector2(-1, +1);
                    break;
                case QuadTreeDirection.RightUp:
                    step = new Vector2(+1, +1);
                    break;
                case QuadTreeDirection.RightBottom:
                    step = new Vector2(+1, -1);
                    break;
            }

            var child = new QuadTree(new Rect2D(center + step * halfsize / 2, halfsize / 2), capacity);
            child.parent = this;
            child.currDir = qtd;

            childs.Add(qtd, child);
        }

        public bool Insert(Circle2D o)
        {
            if (!range.Contains(o.GetCenter())) return false;

            if (data.Count < capacity)
            {
                data.Add(o);
                return true;
            }

            if (!childs.ContainsKey(QuadTreeDirection.LeftBottom))
            {
                Subdivide(QuadTreeDirection.LeftBottom);
            }

            if (childs[QuadTreeDirection.LeftBottom].Insert(o))
            {
                return true;
            }

            if (!childs.ContainsKey(QuadTreeDirection.LeftUp))
            {
                Subdivide(QuadTreeDirection.LeftUp);
            }

            if (childs[QuadTreeDirection.LeftUp].Insert(o))
            {
                return true;
            }

            if (!childs.ContainsKey(QuadTreeDirection.RightUp))
            {
                Subdivide(QuadTreeDirection.RightUp);
            }

            if (childs[QuadTreeDirection.RightUp].Insert(o))
            {
                return true;
            }

            if (!childs.ContainsKey(QuadTreeDirection.RightBottom))
            {
                Subdivide(QuadTreeDirection.RightBottom);
            }

            if (childs[QuadTreeDirection.RightBottom].Insert(o))
            {
                return true;
            }

            return false;
        }

        public void Update()
        {
            var tmpData = new List<Circle2D>(data);

            foreach (var item in tmpData)
            {
                if (range.Contains(item.GetCenter()))
                {
                    foreach (var child in childs.Values)
                    {
                        if (child.range.Contains(item.GetCenter()))
                        {
                            data.Remove(item);
                            child.Insert(item);
                            break;
                        }
                    }
                }
                else
                {
                    data.Remove(item);
                    QuadTree curParent = this;
                    while ((curParent = curParent.parent) != null)
                    {
                        if (curParent.range.Contains(item.GetCenter()))
                        {
                            curParent.Insert(item);
                            break;
                        }
                    }
                }
            }

            var tmpChilds = childs.Values.ToList();
            if (tmpChilds.Count > 0)
            {
                foreach (var child in tmpChilds)
                {
                    child.Update();
                }
            }
            else
            {
                if (data.Count <= 0)
                {
                    if (parent != null)
                    {
                        parent.childs.Remove(currDir);
                    }
                }
            }
        }

        public List<Circle2D> QueryCircle(Vector2 c, float r)
        {
            var result = new List<Circle2D>();

            if (!range.IntersectCircle(c, r))
            {
                return result;
            }

            foreach (var item in data)
            {
                var diff = item.GetCenter() - c;
                if (diff.LengthSquared <= r * r)
                {
                    result.Add(item);
                }
            }

            foreach (var item in childs.Values)
            {
                result.AddRange(item.QueryCircle(c, r));
            }

            return result;
        }

        public void Render(string targetWorldMatrix, ShaderProgram sp)
        {
            sp.SetUniform(targetWorldMatrix, range.GetTransformation());
            range.Render();

            foreach (var item in childs.Values)
            {
                item.Render(targetWorldMatrix, sp);
            }
        }
    }
}
