import { Plugin } from 'vue'

export const PermissionPlugin: Plugin<PermissionOptions> = {
    install: (app, options) => {
        if (!options || !options.permissionProvider) {
            console.warn("Permission won't work without permissionProvider")
            return;
        }

        app.config.globalProperties.$pm = {
            some(...permissionCodes: string[]) {
                return options.permissionProvider.some(permissionCodes);
            },
            every(...permissionCodes: string[]) {
                return options.permissionProvider.every(permissionCodes);
            },
            has(permissionCode: string) {
                return options.permissionProvider.has(permissionCode);
            }
        }
    }
}

export interface PermissionOptions {
    permissionProvider: { has: (permissionCode: string) => boolean, some: (permissionCodes: string[]) => boolean, every: (permissionCodes: string[]) => boolean };
}