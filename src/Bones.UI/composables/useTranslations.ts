import { ref } from 'vue'
import { useDevMode } from './useDevMode';

const _translations = ref<{ code: string, value: string }[]>([]);

export function useTranslations() {

    const { isDevMode } = useDevMode();

    const $tr = (code: string, defaultValue: string, ...parameters: (string | number)[]): string => {
        if (isDevMode.value) {
            return code;
        }

        let translation = _translations.value.find(t => t.code === code)?.value ?? defaultValue;
        if (translation && parameters.length) {
            for (let i = 0; i < parameters.length; i++) {
                translation = translation.replace(`{${i}}`, parameters[i].toString());
            }
        }
        return translation;
    };

    const set = (translations: { code: string, value: string }[]) => {
        _translations.value = translations;
    }

    return {
        $tr,
        set
    }
}
