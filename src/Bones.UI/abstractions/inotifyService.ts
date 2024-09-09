export interface INotifyService<TDetails> {
    subscribe(event: "add" | "update", callback: AddOrUpdateCallback<TDetails>): number
    subscribe(event: "delete", callback: DeleteCallback): number
    subscribe(event: "reset", callback: ResetCallback): number
    subscribe(event: "all", callback: AllCallback<TDetails>): number
    unsubscribe(id: number): void
}

export type AddOrUpdateEvent = "add" | "update";
export type DeleteEvent = "delete";
export type ResetEvent = "reset";
export type NotifyEvent = AddOrUpdateEvent | DeleteEvent | ResetEvent;
export type AllEvent = NotifyEvent | "all";

export type AddOrUpdateCallback<TDetails> = (ev: AddOrUpdateEvent, payload: TDetails) => void;
export type DeleteCallback = (ev: DeleteEvent, id: any) => void;
export type ResetCallback = (ev: ResetEvent) => void;
export type AllCallback<TDetails> = AddOrUpdateCallback<TDetails> | DeleteCallback | ResetCallback;