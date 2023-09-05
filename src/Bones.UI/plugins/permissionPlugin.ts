import { Plugin } from 'vue'

export const PermissionPlugin: Plugin<PermissionOptions> = {
    install: (app, options) => {
        if (!options || !options.permissionProvider) {
            console.warn("Permission won't work without permissionProvider")
            return;
        }

        app.config.globalProperties.$pm = {
            some(...permissionCodes: string[]) {
                return permissionCodes.some(code => options.permissionProvider.has(code));
            },
            every(...permissionCodes: string[]) {
                return permissionCodes.every(code => options.permissionProvider.has(code));
            }
        }
    }
}

export interface PermissionOptions {
    permissionProvider: { has: (permissionCode: string) => boolean };
}