using System;
using Akka.Actor;

namespace Bones.Akka
{
    public delegate Props Creator(Type t, IUntypedActorContext context);
    public delegate Props Creator<T>(IUntypedActorContext context);
}