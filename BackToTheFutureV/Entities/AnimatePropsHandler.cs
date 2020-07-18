using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Entities
{
    public class AnimatePropsHandler
    {
        private static List<AnimateProp> _animateProps = new List<AnimateProp>();

        public static void AddAnimateProp(AnimateProp prop)
        {
            if (!_animateProps.Contains(prop))
                _animateProps.Add(prop);
        }

        public static void RemoveAnimateProp(AnimateProp prop)
        {
            if (_animateProps.Contains(prop))
                _animateProps.Remove(prop);
        }

        public static void Tick()
        {
            foreach(var animateProp in _animateProps)
            {
                animateProp.ProcessPropExistance();
            }
        }
    }
}
