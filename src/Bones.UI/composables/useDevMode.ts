import { ref, computed } from "vue";

import { EventQueue } from "../core/eventQueue";

const DEV_MODE_TOPIC = 'devMode';
const devMode = ref(false);

EventQueue.instance.subscribe(DEV_MODE_TOPIC, (_topic: string, payload: boolean) => {
    devMode.value = payload;
    console.log(`Dev mode is now ${devMode.value ? 'enabled' : 'disabled'}`);
});

export function useDevMode() {
    const toggleDevMode = () => {
        EventQueue.instance.publish(DEV_MODE_TOPIC, !devMode.value);
    };

    const isDevMode = computed(() => devMode.value);

    return {
        toggleDevMode,
        isDevMode
    };
}