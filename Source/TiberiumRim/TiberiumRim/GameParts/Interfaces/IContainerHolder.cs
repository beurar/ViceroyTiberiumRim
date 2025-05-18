using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiberiumRim
{
    public interface IContainerHolder
    {
        Comp_TiberiumContainer ContainerComp { get; }
        void Notify_ContainerFull();
    }
}
