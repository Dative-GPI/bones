import _ from "lodash";

import { AddOrUpdateEvent, AllCallback, AllEvent, DeleteEvent, INotifyService, NotifyEvent, ResetEvent } from "../abstractions";

import { EventQueue } from "./eventQueue";

export class NotifyService<TDetails> implements INotifyService<TDetails> {
    private counter: number = 0;
    private topic: string;

    private subscribers: Subscriber<TDetails>[] = [];

    constructor(type: string) {
        this.topic = `entity.${type.toLowerCase()}`;
        EventQueue.instance.subscribe<EntityEvent<TDetails>>(this.topic, this.onEntityEvent.bind(this));
    }

    private onEntityEvent(_topic: string, payload: EntityEvent<TDetails>) {
        _(this.subscribers)
            .filter(
                (s: any) =>
                    (s.ev === payload.action || s.ev == "all")
            )
            .forEach((s: any) => {
                try {
                    s.callback(payload.action as never, 'payload' in payload ? payload.payload : undefined);
                } catch (error) {
                    console.error(error);
                }
            });
    }

    public subscribe(
        event: AllEvent,
        callback: AllCallback<TDetails>
    ): number {
        this.counter++;

        this.subscribers.push({
            ev: event,
            callback: callback,
            id: this.counter,
        });

        return this.counter;
    }

    public unsubscribe(id: number): void {
        const index = _.findIndex(this.subscribers, (s: any) => s.id == id);
        if (index != -1) {
            this.subscribers.splice(index, 1);
        }
    }

    public notify(event: AddOrUpdateEvent, payload: TDetails): void;
    public notify(event: DeleteEvent, payload: any): void;
    public notify(event: ResetEvent, payload?: any): void;

    notify(event: NotifyEvent, payload: any) {
        const data: EntityEvent<TDetails> = {
            action: event,
            payload: payload
        };

        EventQueue.instance.publish(this.topic, data)
    }
}

interface Subscriber<TDetails> {
    id: number;
    ev: AllEvent;
    callback: AllCallback<TDetails>;
}

export type EntityEvent<TDetails> = AddEntityEvent<TDetails> | UpdateEntityEvent<TDetails> | DeleteEntityEvent | ResetEntityEvent;

interface AddEntityEvent<TDetails> {
    action: "add";
    payload: TDetails;
}

interface UpdateEntityEvent<TDetails> {
    action: "update";
    payload: TDetails;
}

interface DeleteEntityEvent {
    action: "delete";
    payload: any;
}

interface ResetEntityEvent {
    action: "reset";
}