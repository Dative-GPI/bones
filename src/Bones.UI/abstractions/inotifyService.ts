export interface INotifyService<TDetails> {
    subscribe(event: "add" | "update", callback: AddOrUpdateCallback<TDetails>): number
    subscribe(event: "delete", callback: DeleteCallback): number
    subscribe(event: "all", callback: AllCallback<TDetails>): number
    unsubscribe(id: number): void
}

export type AddOrUpdateEvent = "add" | "update";
export type DeleteEvent = "delete";
export type NotifyEvent = AddOrUpdateEvent | DeleteEvent;
export type AllEvent = AddOrUpdateEvent | DeleteEvent | "all";

export type AddOrUpdateCallback<TDetails> = (ev: AddOrUpdateEvent, payload: TDetails) => void;
export type DeleteCallback = (ev: DeleteEvent, id: any) => void;
export type AllCallback<TDetails> = AddOrUpdateCallback<TDetails> | DeleteCallback;