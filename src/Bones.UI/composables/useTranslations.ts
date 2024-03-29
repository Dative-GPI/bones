import { ref } from 'vue'

const _translations = ref<{ code: string, value: string }[]>([]);

export function useTranslations() {

    const $tr = (code: string, defaultValue: string, ...parameters: (string | number)[]): string => {
        let translation = _translations.value.find(t => t.code === code)?.value ?? defaultValue;
        if (translation && parameters.length) {
            for (let p of parameters) {
                translation = translation.replace(`{${parameters.indexOf(p)}}`, p.toString());
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
