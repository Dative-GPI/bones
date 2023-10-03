import { Ref } from "vue";
import { AllCallback, NotifyEvent } from "../abstractions/inotifyService";

export function onCollectionChanged<TInfos, TDetails extends TInfos>(
    entities: Ref<TInfos[]>,
    filter: (el: TDetails) => boolean = (el: TDetails) => true,
    identifier: (e1: TInfos) => any = e1 => (e1 as any).id): AllCallback<TDetails> {

    const result: AllCallback<TDetails> = (ev: NotifyEvent, payload: TDetails | any) => {
        switch (ev) {
            case "add":
                add(entities, payload as TDetails, identifier, filter);
                return;
            case "update":
                update(entities, payload as TDetails, identifier, filter);
                return;
            case "delete":
                remove(entities, payload, identifier);
                return;
        }
    }

    return result;
}

export function onEntityChanged<TDetails>(
    entity: Ref<TDetails | null>,
    identifier: (e1: TDetails) => any = e1 => (e1 as any).id): AllCallback<TDetails> {

    const result: AllCallback<TDetails> = (ev: NotifyEvent, payload: any) => {
        if (!entity.value) return;
        const id = identifier(entity.value);

        if (ev === "add" || ev === "update") {
            if (id === identifier(payload)) {
                entity.value = payload;
            }
        }

        if (ev === 'delete' && id === payload) {
            entity.value = null;
        }
    }

    return result;
}

function add<TInfos, TDetails extends TInfos>(
    entities: Ref<TInfos[]>,
    payload: TDetails,
    identifier: (e1: TInfos) => any,
    filter: (el: TDetails) => boolean
) {
    const collection = entities.value;
    const payloadId = identifier(payload);
    const index = collection.findIndex(el => identifier(el) === payloadId);
    const shouldBeAdded = filter(payload);

    if (index == -1 && shouldBeAdded) {
        collection.push(payload);
    }
    else if (index == -1 && !shouldBeAdded) {
        // nothing to do
    }
    else if (index != -1 && !shouldBeAdded) {
        collection.splice(index, 1);
    }
    else { // index != -1 && shouldBeAdded
        collection.splice(index, 1, payload);
    }
}

function update<TInfos, TDetails extends TInfos>(
    entities: Ref<TInfos[]>,
    payload: TDetails,
    identifier: (e1: TInfos) => any,
    filter: (el: TDetails) => boolean
) {
    const collection = entities.value;
    const payloadId = identifier(payload);
    const index = collection.findIndex(el => identifier(el) === payloadId);
    const shouldBeUpdated = filter(payload);

    if (index != -1 && shouldBeUpdated) {
        collection.splice(index, 1, payload);
    }
    else if (index != -1 && !shouldBeUpdated) {
        collection.splice(index, 1);
    }
    else if (index == -1 && shouldBeUpdated) {
        collection.push(payload);
    }
    else {
        // index == -1 && !shouldBeUpdated
    }
}

function remove<TInfos>(
    entities: Ref<TInfos[]>,
    payload: any,
    identifier: (e1: TInfos) => any
) {
    const collection = entities.value;
    const index = collection.findIndex(el => identifier(el) === payload);
    if (index != -1) {
        collection.splice(index, 1);
    }
}

