using System;
using Akka.Actor;

namespace Bones.Akka
{
    public delegate Props Creator(Type t, IActorContext context);
    public delegate Props Creator<out T>(IActorContext context);
}
