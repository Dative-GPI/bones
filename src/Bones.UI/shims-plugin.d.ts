import Vue from "vue";

declare module "vue" {
    interface ComponentCustomProperties {
        $tr: (code: string, defaultLabel: string, ...parameters: string[]) => string;
        $pm: {
            some(...permissionCodes: string[]): boolean;
            every(...permissionCodes: string[]): boolean;
        };
    }
}

declare global {
    interface Window {
        _bonesQueue: any;
    }
}