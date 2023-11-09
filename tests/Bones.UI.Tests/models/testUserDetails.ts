export class TestUserDetails {
    id: string;
    label: string;

    constructor(dto: TestUserDetailsDTO) {
        this.id = dto.id;
        this.label = dto.label;
    }
} 

export interface TestUserDetailsDTO {
    id: string;
    label: string;
}

export interface CreateTestUserDTO {
    label: string;
}

export interface UpdateTestUserDTO {
    label: string;
}