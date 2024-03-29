import { Ref, onUnmounted, ref } from "vue";

import { FilterFactory } from "./filterFactory";
import { INotifyService } from "../abstractions";
import { onCollectionChanged, onEntityChanged } from "../tools";

export class ComposableFactory {
    public static get<TDetails>(factory: () => { get(id: string): Promise<TDetails> } & INotifyService<TDetails>, applyFactory?: () => (entity: TDetails) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            const service = factory();
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
                    if (apply) apply(entity.value);
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

    public static getMany<TDetails extends TInfos, TInfos, TFilter>(factory: () => { getMany(filter?: TFilter): Promise<TInfos[]> } & INotifyService<TDetails>, applyFactory?: () => (entity: TInfos) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            const service = factory();
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
                    if (apply) {
                        for (const entity of entities.value) {
                            apply(entity);
                        }
                    }
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

    public static fetch<TDetails>(factory: () => { fetch(): Promise<TDetails> } & INotifyService<TDetails>, applyFactory?: () => (entity: TDetails) => void) {
        return () => {
            const apply = applyFactory ? applyFactory() : () => { };
            const service = factory();
            let subscribersIds: number[] = [];

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const fetching = ref(false);
            const entity = ref<TDetails | null>(null) as Ref<TDetails | null>;

            const fetch = async () => {
                fetching.value = true;
                try {
                    entity.value = await service.fetch();
                    if (apply) apply(entity.value);
                }
                finally {
                    fetching.value = false;
                }

                subscribersIds.push(service.subscribe("all", onEntityChanged(entity, apply)));

                return entity;
            }

            return {
                fetching: fetching,
                fetch,
                entity: entity
            }
        }
    }

    public static sync<TDetails extends TInfos, TInfos>(factory: () => INotifyService<TDetails>) {
        return () => {
            const service = factory();
            let subscribersIds: number[] = [];

            const synceds = ref([]) as Ref<TInfos[]>;

            onUnmounted(() => {
                subscribersIds.forEach(id => service.unsubscribe(id));
                subscribersIds = [];
            });

            const sync = <TFilter>(entities: TInfos[], filter?: TFilter, customFilter?: (el: TDetails) => boolean) => {
                synceds.value = entities;

                const filterMethod = customFilter || (filter ? FilterFactory.create(filter) : (el: TInfos) => true);

                subscribersIds.push(service.subscribe("all", onCollectionChanged(synceds, filterMethod)));
            }

            return {
                synceds,
                sync
            }
        }
    }

    public static create<TCreateDTO, TDetails>(factory: () => { create(payload: TCreateDTO): Promise<TDetails> } & INotifyService<TDetails>) {
        return () => {
            const service = factory();
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

    public static update<TUpdateDTO, TDetails>(factory: () => { update(id: string, payload: TUpdateDTO): Promise<TDetails> } & INotifyService<TDetails>) {
        return () => {
            const service = factory();
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

    public static remove(factory: () => { remove(id: string): Promise<void> }) {
        return () => {
            const service = factory();

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