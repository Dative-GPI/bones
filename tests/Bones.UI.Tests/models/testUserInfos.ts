export class TestUserInfos {
    id: string;
    label: string;

    constructor(dto: TestUserInfosDTO) {
        this.id = dto.id;
        this.label = dto.label;
    }
} 

export interface TestUserInfosDTO {
    id: string;
    label: string;
}

export interface TestUserFilter {
    label?: string;
}