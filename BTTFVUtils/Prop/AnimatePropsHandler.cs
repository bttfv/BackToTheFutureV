using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionLibrary
{
    public class AnimatePropsHandler
    {
        private static List<AnimateProp> _animateProps = new List<AnimateProp>();

        internal static void AddAnimateProp(AnimateProp prop)
        {
            if (!_animateProps.Contains(prop))
                _animateProps.Add(prop);
        }

        internal static void RemoveAnimateProp(AnimateProp prop)
        {
            if (_animateProps.Contains(prop))
                _animateProps.Remove(prop);
        }

        internal static void Process()
        {
            foreach(var animateProp in _animateProps)
                animateProp.ProcessPropExistance();
        }
    }
}
