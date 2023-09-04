import { PluginFunction, PluginObject } from 'vue'

const install: PluginFunction<TranslationOptions> = (vue, options) => {
    if (!options || !options.translationProvider) {
        console.warn("Translation won't work without translationProvider")
        return;
    }

    vue.prototype.$tr = (codeLabel: string, defaultLabel: string) => {
        return options.translationProvider.get(codeLabel) ?? defaultLabel;
    };
}

export const TranslationPlugin: PluginObject<TranslationOptions> = {
    install
}

export interface TranslationOptions {
    translationProvider: { get: (codeLabel: string) => string | null };
}