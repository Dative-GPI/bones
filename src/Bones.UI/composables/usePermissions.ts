import { ref } from 'vue'

const _permissions = ref<string[]>([]);
const _setted = false;

export function usePermissions() {

    const some = (...permissionCodes: string[]) => {
        if (!_setted) {
            console.warn("Permissions not setted yet");
        }
        return _permissions.value.some(p => permissionCodes.includes(p));
    }

    const every = (...permissionCodes: string[]) => {
        if (!_setted) {
            console.warn("Permissions not setted yet");
        }
        return permissionCodes.every(p => _permissions.value.includes(p));
    }

    const has = (permissionCode: string) => {
        if (!_setted) {
            console.warn("Permissions not setted yet");
        }
        return _permissions.value.includes(permissionCode);
    }

    const set = (permissions: string[]) => {
        _permissions.value = permissions;
    }

    return {
        some,
        every,
        has,
        set
    }
}
