import { Plugin } from "vue";

import { useTranslations } from '../composables';

export const TranslationPlugin: Plugin = {
    install: (app) => {
        const { $tr } = useTranslations();

        app.config.globalProperties.$tr = $tr;
    }
}