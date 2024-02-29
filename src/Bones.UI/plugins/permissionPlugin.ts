import { Plugin } from "vue";

import { usePermissions } from '../composables/usePermissions';

export const PermissionPlugin: Plugin = {
    install: (app) => {
        const { some, every } = usePermissions();

        app.config.globalProperties.$pm = {
            some,
            every
        }
    }
}