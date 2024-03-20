export class TestUserDetails {
    id: string;
    label: string;
    childs: TestUserChildInfos[]

    constructor(dto: TestUserDetailsDTO) {
        this.id = dto.id;
        this.label = dto.label;
        this.childs = [];
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

export class TestUserChildInfos {

}