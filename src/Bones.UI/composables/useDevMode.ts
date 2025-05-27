import { ref, computed } from "vue";

import { EventQueue } from "../core/eventQueue";

const devMode = ref(false);

EventQueue.instance.subscribe('devMode', (_topic: string, payload: boolean) => {
    devMode.value = payload;
    console.log(`Dev mode is now ${devMode.value ? 'enabled' : 'disabled'}`);
});

export function useDevMode() {
    const toggleDevMode = () => {
        EventQueue.instance.publish('devMode', !devMode.value);
    };

    const isDevMode = computed(() => devMode.value);

    return {
        toggleDevMode,
        isDevMode
    };
}