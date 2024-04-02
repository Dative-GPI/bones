import { Ref, onUnmounted, ref } from "vue";

import { FilterFactory } from "./filterFactory";
import { INotifyService } from "../abstractions";
import { onCollectionChanged, onEntityChanged } from "../tools";

type CFunc<TName extends string, TArgs, TResult> = Record<TName, (args: TArgs) => Promise<TResult>>

export class ComposableFactory {
    public static get<TDetails>(service: { get(id: string): Promise<TDetails> } & INotifyService<TDetails>, applyFactory?: () => (entity: Ref<TDetails>) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const getting = ref(false);
            const entity = ref<TDetails | null>(null) as Ref<TDetails | null>;

            const get = async (id: string) => {
                getting.value = true;
                try {
                    entity.value = await service.get(id);
                    if (apply) apply(entity as Ref<TDetails>);
                }
                finally {
                    getting.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(entity, apply)));

                return entity;
            }

            return {
                getting: getting,
                get,
                entity: entity
            }
        }
    }

    public static getMany<TDetails extends TInfos, TInfos, TFilter>(service: { getMany(filter?: TFilter): Promise<TInfos[]> } & INotifyService<TDetails>, applyFactory?: () => (entities: Ref<TInfos[]>) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const fetching = ref(false);
            const entities = ref<TInfos[]>([]) as Ref<TInfos[]>;

            const getMany = async (filter?: TFilter, customFilter?: (el: TDetails) => boolean) => {
                fetching.value = true;
                try {
                    entities.value = await service.getMany(filter);
                    if (apply) apply(entities)
                }
                finally {
                    fetching.value = false;
                }

                const filterMethod = customFilter || (filter ? FilterFactory.create(filter) : (el: TInfos) => true);

                subscribersIds.push(service.subscribe("all", onCollectionChanged(entities, filterMethod)));

                return entities;
            }

            return {
                fetching: fetching,
                getMany,
                entities: entities
            }
        }
    }

    public static custom<TDetails, TArgs extends any[]>(method: (...args: TArgs) => Promise<TDetails>, applyFactory?: () => (entity: Ref<TDetails>) => void) {
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

    public static create<TCreateDTO, TDetails>(service: { create(payload: TCreateDTO): Promise<TDetails> } & INotifyService<TDetails>, applyFactory?: () => (entity: Ref<TDetails>) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const creating = ref(false);
            const created = ref<TDetails | null>(null) as Ref<TDetails | null>;

            const create = async (payload: TCreateDTO) => {
                creating.value = true;
                try {
                    created.value = await service.create(payload);
                    if (apply) apply(created as Ref<TDetails>);
                }
                finally {
                    creating.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(created)));

                return created as Ref<TDetails>;
            }

            return {
                creating: creating,
                create,
                created: created
            }
        }
    }

    public static update<TUpdateDTO, TDetails>(service: { update(id: string, payload: TUpdateDTO): Promise<TDetails> } & INotifyService<TDetails>, applyFactory?: () => (entity: Ref<TDetails>) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const updating = ref(false);
            const updated = ref<TDetails | null>(null) as Ref<TDetails | null>;

            const update = async (id: string, payload: TUpdateDTO) => {
                updating.value = true;
                try {
                    updated.value = await service.update(id, payload);
                    if (apply) apply(updated as Ref<TDetails>);
                }
                finally {
                    updating.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(updated)));

                return updated.value as Ref<TDetails>;
            }

            return {
                updating: updating,
                update,
                updated: updated
            }
        }
    }

    public static remove(service: { remove(id: string): Promise<void> }) {
        return () => {
            const removing = ref(false);

            const remove = async (id: string) => {
                removing.value = true;
                try {
                    await service.remove(id);
                }
                finally {
                    removing.value = false;
                }
            }

            return {
                removing: removing,
                remove
            }
        }
    }
}