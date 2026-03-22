# Backport des composants DAT'Acquisition vers Bones NuGet

## Contexte

DAT'Acquisition utilise Bones via un git submodule local qui a diverge du repo GitHub/NuGet.
Ce document recense chaque ajout/modification necessaire pour pouvoir supprimer le submodule
et utiliser exclusivement les packages NuGet Bones.

Aucun composant backporté n'est specifique a DAT'Acquisition — tous sont des utilitaires generiques.

---

## 1. Bones.Akka — InitializeActor

**Action** : AJOUTER `InitializeActor.cs`

**Justification** : Pattern standard Akka pour les acteurs necessitant une initialisation asynchrone
avec stash des messages et retry avec backoff lineaire. Utilise par 32 acteurs dans DAT'Acquisition.
Ce pattern est documente dans la litterature Akka et n'a aucun couplage metier.

---

## 2. Bones.Akka — RestartException

**Action** : AJOUTER `Exceptions/RestartException.cs`

**Justification** : Exception marker pour les strategies de supervision Akka
("si RestartException → restart l'acteur"). Pattern standard de supervision, 15 fichiers l'utilisent.

---

## 3. Bones.Akka — DisableMessage

**Action** : AJOUTER `Messages/DisableMessage.cs` (singleton)

**Justification** : Message de controle lifecycle acteur complementaire a RestartMessage.
Pattern singleton pour eviter les allocations inutiles (best practice Akka pour les messages sans donnees).

---

## 4. Bones.Akka — RestartMessage

**Action** : MODIFIER `Messages/RestartMessage.cs` — ajouter le pattern singleton

**Justification** : Best practice Akka — les messages sans donnees doivent etre des singletons
pour eviter les allocations. 7 fichiers utilisent `RestartMessage.Instance`.

---

## 5. Bones.Akka — Creator delegates

**Action** : MODIFIER `Creator.cs` — ajouter la covariance `out T` sur `Creator<T>`

**Justification** : La covariance permet d'utiliser un `Creator<ConcreteActor>` la ou un
`Creator<IActorInterface>` est attendu, ce qui est essentiel pour le pattern `AddCreator<TInterface, TActor>()`.
On garde `IActorContext` (plus generique que `IUntypedActorContext`) et `RootCreator<T>` du NuGet actuel.

---

## 6. Bones.Akka — DependencyInjector

**Action** : MODIFIER `DI/DependencyInjector.cs` — ajouter la double registration dans `AddCreator<TInterface, TActor>()`

**Justification** : Quand on enregistre `AddCreator<IDeviceActor, DeviceActor>()`, il faut
pouvoir resoudre aussi bien `Creator<IDeviceActor>` que `Creator<DeviceActor>` (pour les tests).
Le NuGet actuel n'enregistre que l'interface.

---

## 7. Bones.Converters — ToFloat, ToHalf, ToInt/ToUInt sans startIndex, StringConverter

**Action** : MODIFIER `EndianBitConverter.cs` + AJOUTER `StringConverter.cs`

**Justification** :
- `ToFloat(byte[])` : dispatch automatique vers Half/Single/Double selon la taille — essentiel pour les protocoles industriels ou la taille du registre varie
- `ToHalf(byte[], int)` : decodage IEEE 754 half-precision (16 bits) — standard float16
- `ToInt(byte[])` / `ToUInt(byte[])` sans startIndex : surcharges de commodite + support des tailles impaires (3, 5, 6, 7 octets) — les registres Modbus et protocoles IoT utilisent des tailles non standard
- `StringConverter` : conversion hex string ↔ byte array — utilitaire de base pour le debug protocole

Tout est du pur calcul binaire sans couplage metier.

---

## 8. Bones.Flow — ChainOfResponsibility

**Action** : AJOUTER `Core/ChainOfResponsibility/` (4 fichiers) + interfaces (3 fichiers)

**Justification** : Implementation du design pattern Chain of Responsibility, integre au systeme
de pipeline Bones.Flow. Deux variantes : `<TRequest>` (sans resultat) et `<TRequest, TResult>` (avec resultat).
Implemente `IMiddleware` pour etre composable avec Pipeline via `.With(cor)` / `.Add(cor)`.
Utilise par 15 fichiers dans DAT'Acquisition (handlers de contexte, publishers gateway).
Le pattern est un GoF classique, l'implementation ne contient aucun couplage metier.

---

## 9. Bones.Grpc — Interceptors et Extensions

**Action** : AJOUTER `DI/DependencyInjector.cs`,
`Extensions/ByteStringExtensions.cs`, `Interceptors/DeadlineInterceptor.cs`,
`Interceptors/StreamDeadlineInterceptor.cs`

**Justification** : Infrastructure gRPC standard :
- `DeadlineInterceptor` : ajoute un deadline de 5s aux appels unaires — best practice gRPC pour eviter les appels pendants
- `StreamDeadlineInterceptor` : deadline de 30min pour le streaming serveur
- `ByteStringExtensions` : conversion `byte[] → ByteString` (commodite Protobuf)
- Le `NotFoundInterceptor` existe deja dans le NuGet

Chaque applicatif definit sa propre methode d'extension pour composer les interceptors souhaites.

Tout est de l'infrastructure gRPC generique.

---

## 10. Bones.Selectors — Projet complet

**Action** : AJOUTER le projet `Bones.Selectors/` (6 fichiers)

**Justification** : Utilitaire de selection de donnees dans JSON (dot-path) et XML (XPath).
Pattern generique utilise pour l'extraction de valeurs dans des payloads de protocoles.
Aucun couplage metier — c'est l'equivalent d'un JSONPath simplifie.

---

## 11. Bones.Akka.Tests — Helpers de test

**Action** : AJOUTER `AkkaTestClass.cs`, `AkkaActorWrapper.cs`, `ProxyNodeActor.cs`
dans `src/Bones.Akka.Tests/` (librairie, pas projet de test)

**Justification** : Infrastructure de test Akka reutilisable :
- `AkkaTestClass` : classe de base xUnit/TestKit avec helpers (`CreateProxyCreator`, `CreateLogger`, `CreateWrappedActor`)
- `AkkaActorWrapper` : wrapper pour verifier l'existence d'acteurs/enfants dans les tests
- `ProxyNodeActor` : acteur proxy qui forward vers un TestProbe — pattern standard pour mocker les enfants

Le projet de test actuel sur NuGet est un placeholder vide (`Test1() { }`).

---

## 12. Bones.Akka — BaseManager (depuis DAT-Foundation)

**Action** : AJOUTER `BaseManager.cs`

**Justification** : Pattern generique de routage de messages vers des acteurs enfants dynamiques.
L'acteur parent recoit un message, extrait un nom d'enfant (abstract `GetChildName`), cree
l'enfant via Creator<T> s'il n'existe pas, puis forward le message. Ce pattern est utilise
dans DAT-Foundation (DevicesManagerActor, etc.) et correspond exactement a ce que font
DevicesManager, PollersManager, etc. dans DAT-Acquisition — sans la formalisation.

Pattern GoF Mediator/Router, completement generique.

---

## 13. Bones.Akka — DebouncedActor (depuis DAT-Foundation)

**Action** : AJOUTER `DebouncedActor.cs`

**Justification** : Classe de base pour les acteurs qui ont besoin de debouncer des messages
(ne traiter que le dernier message recu dans une fenetre de temps). Utilise les Timers Akka.
Pattern generique utile pour les gateways OPC-UA, les watchers de configuration, etc.

Variante standalone (pas liee a BaseWorker) pour plus de flexibilite.

---

## 14. FakeEntity — Pas de modification

**Action** : AUCUNE — la version NuGet (generique `FakeEntity<T>`) est superieure a la version locale (non-generique `FakeEntity`).
C'est DAT'Acquisition qui devra migrer vers `FakeEntity<T>`.

---

## Composant NON backporte

### MicrosoftDependencyResolver.cs

**Action** : NE PAS backporter

**Justification** : Implementation custom de `IDependencyResolver` (Akka.DI.Core deprecated).
Le NuGet utilise deja `Akka.DependencyInjection` (API moderne). Le code local est 129 lignes
de plomberie que l'API standard rend inutile.
