import _ from "lodash";
import Ajv, { JSONSchemaType, ValidateFunction } from "ajv";

import { uuidv4 } from "../tools";

interface WindowsMessage {
    id: string;
    topic: string;
    payload: object;
}

const schema: JSONSchemaType<WindowsMessage> = {
    type: "object",
    properties: {
        id: { type: "string" },
        topic: { type: "string" },
        payload: { type: "object" }
    },
    required: ["id", "topic", "payload"],
    additionalProperties: false
}

const bufferSize = 100;

export class EventQueue {
    private subscriptionCounter: number;
    private messageCounter: number;
    private buffer: string[];
    private subscribers: EventQueueSubscriber[];
    private validator: ValidateFunction<WindowsMessage>;

    private constructor() {
        this.subscriptionCounter = 0;
        this.subscribers = [];
        this.messageCounter = 0;

        this.validator = new Ajv().compile(schema);
        this.buffer = [...Array(bufferSize)];

        window.addEventListener(
            "message",
            this.onWindowsMessage.bind(this),
            false
        );
    }

    public static get instance(): EventQueue {
        if (!window._bonesQueue) {
            window._bonesQueue = new EventQueue();
        }

        return window._bonesQueue;
    }

    public publish(topic: string, payload: any): void {

        this.publishInternal(topic, payload);

        if (window.top && window.top !== window.self) {
            const id = uuidv4();

            const message: WindowsMessage = {
                id,
                topic,
                payload
            };

            this.buffer[this.messageCounter % bufferSize] = message.id;
            this.messageCounter++;

            window.top.postMessage(JSON.stringify(message), "*");
        }
    }

    public subscribe<T>(topic: string, callback: (topic: string, payload: T) => void): number {
        this.subscriptionCounter++;

        this.subscribers.push({
            id: this.subscriptionCounter,
            topic,
            callback
        });

        return this.subscriptionCounter;
    }

    public unsubscribe(id: number): void {
        const index = _.findIndex(this.subscribers, (s: EventQueueSubscriber) => s.id == id);
        if (index != -1) {
            this.subscribers.splice(index, 1);
        }
    }

    private publishInternal(topic: string, payload: any) {
        _(this.subscribers)
            .filter((s) => s.topic === topic || s.topic === "*")
            .forEach((s) => {
                try {
                    s.callback(topic, payload);
                } catch (error) {
                    console.error(error);
                }
            });
    }

    private onWindowsMessage(event: MessageEvent) {
        let data;

        try {
            data = JSON.parse(event.data);
        } catch (error) {
            return;
        }

        // not with the expected format
        if (!this.validator(data)) {
            return;
        }

        // we already processed this message
        if (this.buffer.includes(data.id)) {
            return;
        }

        this.publishInternal(data.topic, data.payload);
    }
}

interface EventQueueSubscriber {
    id: number;
    topic: string;
    callback: (topic: string, msg: any) => void;
}