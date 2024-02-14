import { Plugin } from "vue";

export const TranslationPlugin: Plugin<TranslationOptions> = {
    install: (app, options) => {
        if (!options || !options.translationsProvider) {
            console.warn("Translation won't work without translationProvider")
            return;
        }

        app.config.globalProperties.$tr = options.translationsProvider.$tr;
    }
}

export interface TranslationOptions {
    translationsProvider: { $tr: (code: string, defaultLabel: string, ...parameters: string[]) => string }
}