using System;
using Akka.Actor;

namespace Bones.Akka
{
    public delegate Props Creator(Type t, IActorContext context);
    public delegate Props Creator<T>(IActorContext context);
    public delegate Props RootCreator<T>(ActorSystem context);
}