import { Ref, onUnmounted, ref } from "vue";

import { FilterFactory } from "./filterFactory";
import { INotifyService } from "../abstractions";
import { onCollectionChanged, onEntityChanged } from "../tools";

export class ComposableFactory {
    public static get<TDetails>(factory: () => { get(id: string): Promise<TDetails> } & INotifyService<TDetails>) {

        const service = factory();

        return () => {
            const getting = ref(false);
            const entity = ref<TDetails | null>(null) as Ref<TDetails | null>;

            const get = async (id: string) => {
                getting.value = true;
                try {
                    entity.value = await service.get(id);
                }
                finally {
                    getting.value = false;
                }

                const subscriberId = service.subscribe("all", onEntityChanged(entity))
                onUnmounted(() => service.unsubscribe(subscriberId));

                return entity;
            }

            return {
                getting: getting,
                get,
                entity: entity
            }
        }
    }

    public static getMany<TInfos, TFilter>(factory: () => { getMany(filter?: TFilter): Promise<TInfos[]> } & INotifyService<TInfos>) {

        const service = factory();

        return () => {
            const fetching = ref(false);
            const entities = ref<TInfos[]>([]) as Ref<TInfos[]>;

            const getMany = async (filter?: TFilter, customFilter?: (el: TInfos) => boolean) => {
                fetching.value = true;
                try {
                    entities.value = await service.getMany(filter);
                }
                finally {
                    fetching.value = false;
                }

                const filterMethod = customFilter || (filter ? FilterFactory.create(filter) : (el: TInfos) => true);

                const subscriberId = service.subscribe("all", onCollectionChanged(entities, filterMethod))
                onUnmounted(() => service.unsubscribe(subscriberId));

                return entities;
            }

            return {
                fetching: fetching,
                getMany,
                entities: entities
            }
        }
    }


    public static create<TCreateDTO, TDetails>(factory: () => { create(payload: TCreateDTO): Promise<TDetails> } & INotifyService<TDetails>) {

        const service = factory();

        return () => {
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

                const subscriberId = service.subscribe("all", onEntityChanged(created))
                onUnmounted(() => service.unsubscribe(subscriberId));

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
        const service = factory();

        return () => {
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

                const subscriberId = service.subscribe("all", onEntityChanged(updated))
                onUnmounted(() => service.unsubscribe(subscriberId));

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
        const service = factory();

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