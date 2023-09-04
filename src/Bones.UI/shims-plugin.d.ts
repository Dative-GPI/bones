import Vue from 'vue';

declare module 'vue/types/vue' {
    interface Vue {
        $tr: (codeLabel: string, defaultLabel: string) => string;
        $pm: {
            some(...permissionCodes: string[]): boolean;
            every(...permissionCodes: string[]): boolean;
        };
    }
}