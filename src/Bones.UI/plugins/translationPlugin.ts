import { Plugin } from 'vue'

export const TranslationPlugin: Plugin<TranslationOptions> = {
    install: (app, options) => {
        if (!options || !options.translationProvider) {
            console.warn("Translation won't work without translationProvider")
            return;
        }

        app.config.globalProperties.$tr = (codeLabel: string, defaultLabel: string) => {
            return options.translationProvider.get(codeLabel) ?? defaultLabel;
        };
    }
}

export interface TranslationOptions {
    translationProvider: { get: (codeLabel: string) => string | null };
}