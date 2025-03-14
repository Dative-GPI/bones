import { Ref, onUnmounted, ref, computed } from "vue";

import { onCollectionChanged, onEntityChanged } from "../tools";
import { INotifyService } from "../abstractions";
import { FilterFactory } from "./filterFactory";

export class ComposableFactory {
    public static get<TDetails>(
        service: { get(id: string, cancellationToken?: AbortController): Promise<TDetails> } & INotifyService<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void,
        allowCancellation: boolean = true
    ) {
        return ComposableFactory.customGet(service, service.get, applyFactory, allowCancellation);
    }

    public static getMany<TDetails extends TInfos, TInfos, TFilter>(
        service: { getMany(filter?: TFilter, cancellationToken?: AbortController): Promise<TInfos[]> } & INotifyService<TDetails>,
        applyFactory?: () => (entities: Ref<TInfos[]>) => void,
        allowCancellation: boolean = true
    ) {
        return ComposableFactory.customGetMany(service, service.getMany, applyFactory, allowCancellation);
    }

    public static create<TCreateDTO, TDetails>(
        service: { create(payload: TCreateDTO, cancellationToken?: AbortController): Promise<TDetails> } & INotifyService<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void,
        allowCancellation: boolean = true
    ) {
        return ComposableFactory.customCreate(service, service.create, applyFactory, allowCancellation);
    }

    public static update<TUpdateDTO, TDetails>(
        service: { update(id: string, payload: TUpdateDTO, cancellationToken?: AbortController): Promise<TDetails> } & INotifyService<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void,
        allowCancellation: boolean = true
    ) {
        return ComposableFactory.customUpdate(service, service.update, applyFactory, allowCancellation);
    }

    public static remove(
        service: { remove(id: string, cancellationToken?: AbortController): Promise<void> },
        allowCancellation: boolean = true
    ) {
        return ComposableFactory.customRemove(service.remove, allowCancellation);
    }

    public static subscribe<TDetails>(service: INotifyService<TDetails>) {
        return () => {
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const subscribe: INotifyService<TDetails>["subscribe"] = (ev: any, callback: any) => {
                const subscriberId = service.subscribe(ev, callback);
                subscribersIds.push(subscriberId);
                return subscriberId;
            }

            return {
                subscribe
            }
        }
    }

    public static custom<TDetails, TArgs extends any[]>(
        method: (...args: TArgs) => Promise<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void
    ) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };

            const fetching = ref(false);
            const entity = ref<TDetails | null>(null) as Ref<TDetails | null>;

            const fetch = async (...args: TArgs) => {
                fetching.value = true;
                try {
                    entity.value = await method(...args);
                    if (apply) apply(entity as Ref<TDetails>);
                }
                finally {
                    fetching.value = false;
                }

                return entity;
            }

            return {
                fetching: fetching,
                fetch,
                entity: entity
            }
        }
    }

    /**
     * Warning : read the code before using this method, the first argument in the method is used to create a filter
     * The last argument passed to the getMany composable can be a custom filter function that will override the default filter
     * */
    public static customGetMany<TDetails extends TInfos, TInfos, TArgs extends any[]>(
        service: INotifyService<TDetails>,
        method: (...args: [...TArgs, AbortController]) => Promise<TInfos[]>,
        applyFactory?: () => (entities: Ref<TInfos[]>) => void,
        allowCancellation: boolean = true
    ) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const fetching = ref(false);
            const _entities = ref<TInfos[]>([]) as Ref<TInfos[]>;
            let _filter: Ref<(el: TInfos) => boolean> = ref(() => true);

            const entities = computed(() => {
                return _entities.value.filter(e => _filter.value(e))
            });

            let cancellationToken: AbortController | null = null;

            const getMany = async (...args: [...TArgs, ((el: TInfos) => boolean)?]) => {
                if (cancellationToken && allowCancellation) {
                    cancellationToken.abort();
                    cancellationToken = null;
                }
                fetching.value = true;
                cancellationToken = new AbortController();

                let customFilter: ((el: TInfos) => boolean) | undefined = undefined

                if (args.length > 1 && typeof args[args.length - 1] === 'function') {
                    customFilter = args.pop();
                }

                let actualArgs = args as unknown as TArgs;

                try {
                    _entities.value = await method(...actualArgs, cancellationToken);
                    if (apply) apply(_entities)
                }
                finally {
                    cancellationToken = null;
                    fetching.value = false;
                }

                const filterMethod = customFilter || (actualArgs.length > 0 ? FilterFactory.create(actualArgs[0]) : () => true);
                _filter.value = filterMethod

                subscribersIds.push(service.subscribe("all", onCollectionChanged(_entities)));
                subscribersIds.push(service.subscribe("reset", async () => {
                    if (cancellationToken && allowCancellation) {
                        cancellationToken.abort();
                        cancellationToken = null;
                    }
                    fetching.value = true;
                    cancellationToken = new AbortController();
                    try {
                        _entities.value = await method(...actualArgs, cancellationToken);
                        if (apply) apply(_entities)
                    }
                    finally {
                        cancellationToken = null;
                        fetching.value = false;
                    }
                }));

                return entities;
            }

            return {
                fetching: fetching,
                getMany,
                entities: entities,
                cancellationToken
            }
        }
    }

    public static customGet<TDetails, TArgs extends any[]>(
        service: INotifyService<TDetails>,
        method: (...args: [...TArgs, AbortController]) => Promise<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void,
        allowCancellation: boolean = true
    ) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const getting = ref(false);
            const entity = ref<TDetails | null>(null) as Ref<TDetails | null>;

            let cancellationToken: AbortController | null = null;

            const get = async (...args: TArgs) => {
                if (cancellationToken && allowCancellation) {
                    cancellationToken.abort();
                    cancellationToken = null;
                }
                getting.value = true;
                cancellationToken = new AbortController();
                try {
                    entity.value = await method(...args, cancellationToken);
                    if (apply) apply(entity as Ref<TDetails>);
                }
                finally {
                    cancellationToken = null;
                    getting.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(entity, apply)));

                return entity;
            }

            return {
                getting: getting,
                get,
                entity: entity,
                cancellationToken
            }
        }
    }

    public static customCreate<TDetails, TArgs extends any[]>(
        service: INotifyService<TDetails>,
        method: (...args: [...TArgs, AbortController]) => Promise<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void,
        allowCancellation: boolean = true
    ) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const creating = ref(false);
            const created = ref<TDetails | null>(null) as Ref<TDetails | null>;

            let cancellationToken: AbortController | null = null;

            const create = async (...args: TArgs) => {
                if (cancellationToken && allowCancellation) {
                    cancellationToken.abort();
                    cancellationToken = null;
                }
                creating.value = true;
                cancellationToken = new AbortController();
                try {
                    created.value = await method(...args, cancellationToken);
                    if (apply) apply(created as Ref<TDetails>);
                }
                finally {
                    cancellationToken = null;
                    creating.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(created)));

                return created as Ref<TDetails>;
            }

            return {
                creating: creating,
                create,
                created: created,
                cancellationToken
            }
        }
    }

    public static customUpdate<TDetails, TArgs extends any[]>(
        service: INotifyService<TDetails>,
        method: (...args: [...TArgs, AbortController]) => Promise<TDetails>,
        applyFactory?: () => (entity: Ref<TDetails>) => void,
        allowCancellation: boolean = true
    ) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const updating = ref(false);
            const updated = ref<TDetails | null>(null) as Ref<TDetails | null>;

            let cancellationToken: AbortController | null = null;

            const update = async (...args: TArgs) => {
                if (cancellationToken && allowCancellation) {
                    cancellationToken.abort();
                    cancellationToken = null;
                }
                updating.value = true;
                cancellationToken = new AbortController();
                try {
                    updated.value = await method(...args, cancellationToken);
                    if (apply) apply(updated as Ref<TDetails>);
                }
                finally {
                    cancellationToken = null;
                    updating.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(updated)));

                return updated.value as Ref<TDetails>;
            }

            return {
                updating: updating,
                update,
                updated: updated,
                cancellationToken
            }
        }
    }

    public static customRemove<TArgs extends any[]>(
        method: (...args: [...TArgs, AbortController]) => Promise<void>,
        allowCancellation: boolean = true
    ) {
        return () => {
            const removing = ref(false);

            let cancellationToken: AbortController | null = null;

            const remove = async (...args: TArgs) => {
                if (cancellationToken && allowCancellation) {
                    cancellationToken.abort();
                    cancellationToken = null;
                }
                removing.value = true;
                cancellationToken = new AbortController();
                try {
                    await method(...args, cancellationToken);
                }
                finally {
                    cancellationToken = null;
                    removing.value = false;
                }
            }

            return {
                removing: removing,
                remove,
                cancellationToken
            }
        }
    }

    public static sync<TDetails extends TInfos, TInfos>(service: INotifyService<TDetails>) {
        return () => {
            let subscribersIds: number[] = [];


            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const sync = <TFilter>(entities: TInfos[], filter?: TFilter, customFilter?: (el: TDetails) => boolean) => {
                const synceds = ref(entities) as Ref<TInfos[]>;

                const filterMethod = customFilter || (filter ? FilterFactory.create(filter) : (el: TInfos) => true);

                subscribersIds.push(service.subscribe("all", onCollectionChanged(synceds, filterMethod)));

                return synceds;
            }

            return {
                sync
            }
        }
    }

    public static syncRef<TDetails extends TInfos, TInfos>(service: INotifyService<TDetails>) {
        return () => {
            let subscribersIds: number[] = [];


            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const sync = <TFilter>(entities: Ref<TInfos[]>, filter?: TFilter, customFilter?: (el: TDetails) => boolean) => {
                const filterMethod = customFilter || (filter ? FilterFactory.create(filter) : (el: TInfos) => true);

                subscribersIds.push(service.subscribe("all", onCollectionChanged(entities, filterMethod)));

                return entities;
            }

            return {
                sync
            }
        }
    }

    public static track<TDetails>(service: INotifyService<TDetails>) {
        return () => {
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const track = (initialValue: TDetails, setter?: ((entity: TDetails) => void)) => {
                const entity = ref(initialValue) as Ref<TDetails>;

                subscribersIds.push(service.subscribe("all", onEntityChanged(entity, (ref) => setter ? setter(ref.value) : null)));

                return entity;
            }

            return {
                track
            }
        }
    }

    public static trackRef<TDetails>(service: INotifyService<TDetails>) {
        return () => {
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const track = (entity: Ref<TDetails>, onChanged?: ((entity: Ref<TDetails>) => void)) => {
                subscribersIds.push(service.subscribe("all", onEntityChanged(entity, onChanged)));

                return entity;
            }

            return {
                track
            }
        }
    }
}