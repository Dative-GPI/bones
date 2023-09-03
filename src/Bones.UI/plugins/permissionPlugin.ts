import { PluginFunction, PluginObject } from 'vue'

const install: PluginFunction<PermissionOptions> = (vue, options) => {
    if (!options || !options.permissionProvider) {
        console.warn("Permission won't work without permissionProvider")
        return;
    }

    vue.prototype.$pm = {
        some(...permissionCodes: string[]) {
            return permissionCodes.some(code => options.permissionProvider.has(code));
        },
        every(...permissionCodes: string[]) {
            return permissionCodes.every(code => options.permissionProvider.has(code));
        }
    }
}

export const PermissionPlugin: PluginObject<PermissionOptions> = {
    install
}

export interface PermissionOptions {
    permissionProvider: { has: (permissionCode: string) => boolean };
}